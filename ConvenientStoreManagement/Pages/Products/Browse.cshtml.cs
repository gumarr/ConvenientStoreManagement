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
        private readonly IMemberCardService _memberCardService;
        private const int PageSize = 12;

        public BrowseModel(IProductService productService, IOrderService orderService, IMemberCardService memberCardService)
        {
            _productService = productService;
            _orderService = orderService;
            _memberCardService = memberCardService;
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

        /// <summary>
        /// API để tìm kiếm thẻ thành viên theo số điện thoại
        /// </summary>
        public async Task<IActionResult> OnPostSearchMemberAsync([FromBody] SearchMemberRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.PhoneNumber))
                {
                    return new JsonResult(new { success = false, message = "Vui lòng nhập số điện thoại." });
                }

                var member = await _memberCardService.GetMemberByPhoneAsync(request.PhoneNumber);
                
                if (member != null)
                {
                    return new JsonResult(new { success = true, member = new { member.MemberCardId, member.FullName, member.LoyaltyPoints } });
                }

                return new JsonResult(new { success = false, message = "Không tìm thấy thẻ thành viên." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API để tạo thẻ thành viên mới
        /// </summary>
        public async Task<IActionResult> OnPostCreateMemberAsync([FromBody] CreateMemberRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.FullName) || string.IsNullOrWhiteSpace(request?.PhoneNumber))
                {
                    return new JsonResult(new { success = false, message = "Vui lòng nhập đầy đủ thông tin (tên và số điện thoại)." });
                }

                var newMember = await _memberCardService.CreateMemberAsync(request.FullName, request.PhoneNumber, request.Email);
                
                if (newMember != null)
                {
                    return new JsonResult(new { success = true, message = "Tạo thẻ thành viên thành công!", member = new { newMember.MemberCardId, newMember.FullName, newMember.LoyaltyPoints } });
                }

                return new JsonResult(new { success = false, message = "Không thể tạo thẻ thành viên (số điện thoại đã tồn tại?)." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
