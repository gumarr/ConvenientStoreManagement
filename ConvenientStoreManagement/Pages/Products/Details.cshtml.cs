using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConvenientStoreManagement.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IProductService _productService;

        public DetailsModel(IProductService productService)
        {
            _productService = productService;
        }

        public ProductDetailDto Detail { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var detail = await _productService.GetProductDetailAsync(id);
            if (detail is null)
                return NotFound();

            Detail = detail;
            return Page();
        }
    }
}
