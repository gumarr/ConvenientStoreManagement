using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Products
{
    [Authorize(Roles = "Admin,Staff")]
    public class DeleteModel : PageModel
    {
        private readonly IProductService _productService;

        public DeleteModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public ProductViewModel Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Simple approach: re-use GetProducts to fetch the specific one for display
            var result = await _productService.GetProductsAsync(null, null, null, 1, 100);
            var item = result.FirstOrDefault(p => p.ProductId == id);
            
            if (item == null)
            {
                return NotFound();
            }

            Product = item;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var result = await _productService.DeleteProductAsync(id.Value);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete product.";
            }
            
            return RedirectToPage("./Index");
        }
    }
}
