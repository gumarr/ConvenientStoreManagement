using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Products
{
    [Authorize(Roles = "Admin,Staff")]
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;

        public CreateModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public ProductInputModel Input { get; set; } = new ProductInputModel();

        public SelectList CategoryList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            CategoryList = new SelectList(categories, "CategoryId", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var categories = await _productService.GetAllCategoriesAsync();
                CategoryList = new SelectList(categories, "CategoryId", "Name");
                return Page();
            }

            var result = await _productService.CreateProductAsync(Input);
            if (result)
            {
                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(string.Empty, "Failed to create product.");
            var cats = await _productService.GetAllCategoriesAsync();
            CategoryList = new SelectList(cats, "CategoryId", "Name");
            return Page();
        }
    }
}
