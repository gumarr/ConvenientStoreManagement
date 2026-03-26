using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConvenientStoreManagement.Pages.Products
{
    public class BrowseModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private const int PageSize = 12;

        public BrowseModel(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        // ── Bound properties ──────────────────────────────────────────────
        public List<Product>    Products         { get; set; } = new();
        public List<Category>   Categories       { get; set; } = new();
        public string?          CurrentSearch    { get; set; }
        public int?             CurrentCategory  { get; set; }
        public int              PageIndex        { get; set; } = 1;
        public int              TotalPages       { get; set; }
        public int              TotalProducts    { get; set; }

        public async Task OnGetAsync(string? searchString, int? categoryId, int pageIndex = 1)
        {
            CurrentSearch   = searchString?.Trim();
            CurrentCategory = categoryId;
            PageIndex       = Math.Max(1, pageIndex);

            // Fetch categories
            Categories = await _productService.GetCategoriesAsync();

            // Fetch total count for pagination
            TotalProducts = await _productService.GetTotalCountAsync(CurrentCategory, CurrentSearch);
            TotalPages    = (int)Math.Ceiling(TotalProducts / (double)PageSize);

            // Fetch paginated products
            Products = await _productService.GetProductsAsync(CurrentCategory, CurrentSearch, PageIndex, PageSize);
        }

        public async Task<IActionResult> OnPostCheckoutAsync([FromBody] CheckoutViewModel payload)
        {
            if (payload == null || payload.Items == null || payload.Items.Count == 0)
            {
                return new JsonResult(new { success = false, message = "Giỏ hàng trống hoặc dữ liệu không hợp lệ." });
            }

            try
            {
                // Hardcode cashierUserId = 1 for testing as requested
                int cashierUserId = 1;
                bool result = await _orderService.ProcessCheckoutAsync(payload, cashierUserId);
                
                if (result)
                {
                    return new JsonResult(new { success = true, message = "Thanh toán thành công!" });
                }
                
                return new JsonResult(new { success = false, message = "Thanh toán thất bại, vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
