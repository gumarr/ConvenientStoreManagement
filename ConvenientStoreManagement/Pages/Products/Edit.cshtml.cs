using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Products
{
    [Authorize(Roles = "Admin,Staff")]
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;

        public EditModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public ProductInputModel Input { get; set; } = default!;

        public SelectList CategoryList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productService.GetProductForEditAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            Input = product;

            var categories = await _productService.GetAllCategoriesAsync();
            CategoryList = new SelectList(categories, "CategoryId", "Name", Input.CategoryId);
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var categories = await _productService.GetAllCategoriesAsync();
                CategoryList = new SelectList(categories, "CategoryId", "Name", Input.CategoryId);
                return Page();
            }

            var result = await _productService.UpdateProductAsync(Input);
            if (result)
            {
                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(string.Empty, "Failed to update product.");
            var cats = await _productService.GetAllCategoriesAsync();
            CategoryList = new SelectList(cats, "CategoryId", "Name", Input.CategoryId);
            return Page();
        }
    }
}
