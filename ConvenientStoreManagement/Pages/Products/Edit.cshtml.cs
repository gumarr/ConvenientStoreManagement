using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConvenientStoreManagement.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IPricingService _pricingService;

        public EditModel(IProductService productService, IPricingService pricingService)
        {
            _productService = productService;
            _pricingService = pricingService;
        }

        // ── Bound properties ───────────────────────────────────────────────────

        [BindProperty]
        public Product Product { get; set; } = null!;

        /// <summary>
        /// Latest average import price injected into the page for JS preview.
        /// 0 means no import data.
        /// </summary>
        public decimal AvgImportPrice { get; set; }

        public List<SelectListItem> CategoryItems { get; set; } = new();

        // ── Handlers ───────────────────────────────────────────────────────────

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product is null)
                return NotFound();

            Product = product;
            AvgImportPrice = await _productService.GetLatestImportPriceAsync(id);
            await LoadCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload categories and import price for re-display
                AvgImportPrice = await _productService.GetLatestImportPriceAsync(Product.ProductId);
                await LoadCategoriesAsync();
                return Page();
            }

            // Persist product changes
            await _productService.UpdateProductAsync(Product);

            // Recalculate and persist new selling price
            var importPrice = await _productService.GetLatestImportPriceAsync(Product.ProductId);
            if (importPrice > 0)
            {
                var newSellingPrice = _productService.CalculateSellingPrice(importPrice, Product.PriceMultiplier);
                await _pricingService.UpdateSellingPriceAsync(Product.ProductId, newSellingPrice);
            }

            return RedirectToPage("/Products/Details", new { id = Product.ProductId });
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private async Task LoadCategoriesAsync()
        {
            var categories = await _productService.GetCategoriesAsync();
            CategoryItems = categories
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToList();
        }
    }
}
