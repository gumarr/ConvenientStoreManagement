using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        public ImportModel(StoreDbContext context, IInventoryService inventoryService, IProductService productService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _productService = productService;
        }

        [BindProperty]
        public InventoryImportRequest ImportRequest { get; set; } = new InventoryImportRequest();

        [BindProperty]
        public int? ReloadIndex { get; set; }

        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostReloadRowAsync()
        {
            await LoadDropdownsAsync();
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
                    }
                }
                else
                {
                    item.Name = null;
                    item.CategoryId = null;
                    item.Unit = null;
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
                return Page();
            }

            // Bind uploaded files by index from the form
            // Files are posted with name "imageFiles[i]" – we map them back to the items
            var files = Request.Form.Files;

            for (int i = 0; i < ImportRequest.Items.Count; i++)
            {
                var item = ImportRequest.Items[i];

                if (item.ProductId.HasValue && item.ProductId.Value > 0)
                {
                    // Existing product: clear model errors for disabled fields and skip image
                    ModelState.Remove($"ImportRequest.Items[{i}].Name");
                    ModelState.Remove($"ImportRequest.Items[{i}].CategoryId");
                    ModelState.Remove($"ImportRequest.Items[{i}].Unit");
                    ModelState.Remove($"ImportRequest.Items[{i}].ImageFile");
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

                    // Attach uploaded image file to the item
                    var fileKey = $"imageFiles[{i}]";
                    var uploadedFile = files.GetFile(fileKey);
                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        // Validate size
                        if (uploadedFile.Length > 3 * 1024 * 1024)
                        {
                            ModelState.AddModelError($"ImportRequest.Items[{i}].ImageFile",
                                "Image upload failed: file exceeds the 3 MB limit.");
                        }
                        else
                        {
                            // Validate type
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
                // Image validation errors raised by the service layer
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadDropdownsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                await LoadDropdownsAsync();
                return Page();
            }
        }

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
    }
}
