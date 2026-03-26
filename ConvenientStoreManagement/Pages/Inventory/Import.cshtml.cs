using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ConvenientStoreManagement.Pages.Inventory
{
    [Authorize]
    public class ImportModel : PageModel
    {
        private readonly StoreDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly IPricingService _pricingService;

        public ImportModel(StoreDbContext context, IInventoryService inventoryService,
            IProductService productService, IPricingService pricingService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _productService = productService;
            _pricingService = pricingService;
        }

        [BindProperty]
        public InventoryImportRequest ImportRequest { get; set; } = new InventoryImportRequest();

        [BindProperty]
        public int? ReloadIndex { get; set; }

        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Serialised product metadata for JS preview calculation.
        /// Shape: { [productId: string]: { stock: number, avgPrice: number, multiplier: number } }
        /// </summary>
        public string ProductMetaJson { get; set; } = "{}";

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            await LoadProductMetaAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostReloadRowAsync()
        {
            await LoadDropdownsAsync();
            await LoadProductMetaAsync();
            ModelState.Clear();

            if (ImportRequest.Items != null && ReloadIndex.HasValue && ReloadIndex.Value < ImportRequest.Items.Count)
            {
                var item = ImportRequest.Items[ReloadIndex.Value];
                if (item.ProductId.HasValue && item.ProductId.Value > 0)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId.Value);
                    if (product != null)
                    {
                        item.Name = product.Name;
                        item.CategoryId = product.CategoryId;
                        item.Unit = product.Unit;
                        // Multiplier is READ-ONLY for existing products; clear so it's not editable
                        item.PriceMultiplier = null;
                    }
                }
                else
                {
                    item.Name = null;
                    item.CategoryId = null;
                    item.Unit = null;
                    item.PriceMultiplier = null;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ImportRequest.Items == null || !ImportRequest.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Please add at least one item to import.");
                await LoadDropdownsAsync();
                await LoadProductMetaAsync();
                return Page();
            }

            // Bind uploaded files by index from the form
            var files = Request.Form.Files;

            for (int i = 0; i < ImportRequest.Items.Count; i++)
            {
                var item = ImportRequest.Items[i];

                if (item.ProductId.HasValue && item.ProductId.Value > 0)
                {
                    // Existing product: clear model errors for disabled / irrelevant fields
                    ModelState.Remove($"ImportRequest.Items[{i}].Name");
                    ModelState.Remove($"ImportRequest.Items[{i}].CategoryId");
                    ModelState.Remove($"ImportRequest.Items[{i}].Unit");
                    ModelState.Remove($"ImportRequest.Items[{i}].ImageFile");
                    ModelState.Remove($"ImportRequest.Items[{i}].PriceMultiplier");
                }
                else
                {
                    // New product: enforce validation
                    if (string.IsNullOrWhiteSpace(item.Name))
                        ModelState.AddModelError($"ImportRequest.Items[{i}].Name", "Name is required for new products.");

                    if (!item.CategoryId.HasValue || item.CategoryId.Value == 0)
                        ModelState.AddModelError($"ImportRequest.Items[{i}].CategoryId", "Category is required for new products.");

                    if (string.IsNullOrWhiteSpace(item.Unit))
                        ModelState.AddModelError($"ImportRequest.Items[{i}].Unit", "Unit is required for new products.");

                    // Multiplier required for new products
                    if (!item.PriceMultiplier.HasValue || item.PriceMultiplier.Value <= 0)
                        ModelState.AddModelError($"ImportRequest.Items[{i}].PriceMultiplier",
                            "Price multiplier is required for new products and must be > 0.");

                    // Attach uploaded image file to the item
                    var fileKey = $"imageFiles[{i}]";
                    var uploadedFile = files.GetFile(fileKey);
                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        if (uploadedFile.Length > 3 * 1024 * 1024)
                        {
                            ModelState.AddModelError($"ImportRequest.Items[{i}].ImageFile",
                                "Image upload failed: file exceeds the 3 MB limit.");
                        }
                        else
                        {
                            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
                            if (!allowed.Contains(uploadedFile.ContentType, StringComparer.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"ImportRequest.Items[{i}].ImageFile",
                                    "Image upload failed: only jpg, jpeg, png, and webp are allowed.");
                            }
                            else
                            {
                                item.ImageFile = uploadedFile;
                            }
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                await LoadProductMetaAsync();
                return Page();
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                await _inventoryService.CreateReceiptAsync(userId, ImportRequest);

                TempData["SuccessMessage"] = "Inventory imported successfully!";
                return RedirectToPage("/Inventory/Import");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadDropdownsAsync();
                await LoadProductMetaAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                await LoadDropdownsAsync();
                await LoadProductMetaAsync();
                return Page();
            }
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        private async Task LoadDropdownsAsync()
        {
            var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            Products = products.Select(p => new SelectListItem
            {
                Value = p.ProductId.ToString(),
                Text = $"{p.Name} (Stock: {p.Stock})"
            }).ToList();
            Products.Insert(0, new SelectListItem { Value = "", Text = "-- New Product --" });

            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            Categories = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();
        }

        /// <summary>
        /// Builds a JSON dict of product data needed by the JS preview calculator.
        /// </summary>
        private async Task LoadProductMetaAsync()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();

            // Get the latest selling/import price for each product
            var priceDict = await _context.ProductPrices
                .Where(pp => pp.EndDate == null)          // active prices only
                .AsNoTracking()
                .GroupBy(pp => pp.ProductId)
                .Select(g => new { ProductId = g.Key, AvgImportPrice = g.Max(x => x.Price) })
                .ToDictionaryAsync(x => x.ProductId, x => x.AvgImportPrice);

            // Actually we need the *import* price (average), not the selling price.
            // The best source is InventoryReceiptDetails (latest ImportPrice per product).
            var importPriceDict = await _context.InventoryReceiptDetails
                .AsNoTracking()
                .GroupBy(d => d.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    // ImportPrice on the detail IS already the weighted average at time of import
                    LatestAvgImportPrice = g.OrderByDescending(d => d.ReceiptDetailId).Select(d => d.ImportPrice).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.LatestAvgImportPrice);

            var meta = products.ToDictionary(
                p => p.ProductId.ToString(),
                p => new
                {
                    stock = p.Stock,
                    avgPrice = importPriceDict.TryGetValue(p.ProductId, out var ap) ? ap : 0m,
                    multiplier = p.PriceMultiplier
                }
            );

            ProductMetaJson = JsonSerializer.Serialize(meta);
        }
    }
}
