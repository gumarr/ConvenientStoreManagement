using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientStoreManagement.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IPricingService _pricingService;
        private readonly StoreDbContext _context;

        public EditModel(IProductService productService, IPricingService pricingService, StoreDbContext context)
        {
            _productService = productService;
            _pricingService = pricingService;
            _context = context;
        }

        public class UpdateProductRequest
        {
            public int ProductId { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string Unit { get; set; }
            [Required]
            public int CategoryId { get; set; }
            [Required]
            public decimal PriceMultiplier { get; set; }
            public bool Status { get; set; }
            public IFormFile? ImageFile { get; set; }
            public string? ImageUrl { get; set; }
        }

        // ── Bound properties ───────────────────────────────────────────────────

        [BindProperty]
        public UpdateProductRequest Product { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; } // also bind here just in case they use this name directly

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

            Product = new UpdateProductRequest
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Unit = product.Unit,
                CategoryId = product.CategoryId,
                PriceMultiplier = product.PriceMultiplier,
                Status = product.Status,
                ImageUrl = product.ImageUrl
            };

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

            // STEP 3: LOAD EXISTING PRODUCT
            var product = await _context.Products.FindAsync(Product.ProductId);
            if (product == null) return NotFound();

            // Store old multiplier to see if it changed
            var oldMultiplier = product.PriceMultiplier;

            // STEP 4: UPDATE BASIC INFO
            product.Name = Product.Name;
            product.Unit = Product.Unit;
            product.CategoryId = Product.CategoryId;
            product.PriceMultiplier = Product.PriceMultiplier;
            product.Status = Product.Status;

            // Use ImageFile from binding or from Product object
            var file = ImageFile ?? Product.ImageFile;

            // STEP 5: HANDLE IMAGE UPLOAD
            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var path = Path.Combine("wwwroot", "img", "Products");
                
                // Ensure directory exists
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var fullPath = Path.Combine(path, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                product.ImageUrl = "/img/Products/" + fileName;
            }

            // STEP 6: HANDLE PriceMultiplier CHANGE
            if (product.PriceMultiplier != oldMultiplier)
            {
                // Recalculate and persist new selling price
                var importPrice = await _productService.GetLatestImportPriceAsync(product.ProductId);
                if (importPrice > 0)
                {
                    var newSellingPrice = _productService.CalculateSellingPrice(importPrice, product.PriceMultiplier);
                    await _pricingService.UpdateSellingPriceAsync(product.ProductId, newSellingPrice);
                }
            }

            // STEP 7: SAVE CHANGES
            await _context.SaveChangesAsync();

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
