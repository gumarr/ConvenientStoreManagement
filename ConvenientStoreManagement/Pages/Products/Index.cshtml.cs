using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Products
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public PaginatedList<ProductViewModel> Products { get; set; } = default!;
        public SelectList CategoryList { get; set; } = default!;

        public string? CurrentSearch { get; set; }
        public int? CurrentCategory { get; set; }
        public string? CurrentSort { get; set; }

        public async Task OnGetAsync(string? search, int? categoryId, string? sortOrder, int? pageIndex)
        {
            CurrentSearch = search;
            CurrentCategory = categoryId;
            CurrentSort = sortOrder ?? "name_asc";

            int pageSize = 10;
            int pageNumber = pageIndex ?? 1;

            Products = await _productService.GetProductsAsync(search, categoryId, sortOrder, pageNumber, pageSize);
            
            var categories = await _productService.GetAllCategoriesAsync();
            CategoryList = new SelectList(categories, "CategoryId", "Name", categoryId);
        }
    }
}
