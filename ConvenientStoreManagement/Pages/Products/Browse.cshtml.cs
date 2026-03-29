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
                return new JsonResult(new { success = false, message = "Cart is empty or invalid data." });
            }

            try
            {
                // Hardcode cashierUserId = 1 for testing as requested
                int cashierUserId = 1;
                bool result = await _orderService.ProcessCheckoutAsync(payload, cashierUserId);
                
                if (result)
                {
                    return new JsonResult(new { success = true, message = "Checkout successful!" });
                }
                
                return new JsonResult(new { success = false, message = "Checkout failed, please try again." });
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
                    return new JsonResult(new { success = false, message = "Please enter phone number." });
                }

                var member = await _memberCardService.GetMemberByPhoneAsync(request.PhoneNumber);
                
                if (member != null)
                {
                    return new JsonResult(new { success = true, member = new { member.MemberCardId, member.FullName, member.PhoneNumber, member.LoyaltyPoints } });
                }

                return new JsonResult(new { success = false, message = "Member card not found." });
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
                    return new JsonResult(new { success = false, message = "Please enter full info (name and phone)." });
                }

                var newMember = await _memberCardService.CreateMemberAsync(request.FullName, request.PhoneNumber, request.Email);
                
                if (newMember != null)
                {
                    return new JsonResult(new { success = true, message = "Member card created successfully!", member = new { newMember.MemberCardId, newMember.FullName, newMember.PhoneNumber, newMember.LoyaltyPoints } });
                }

                return new JsonResult(new { success = false, message = "Cannot create member card (phone already exists?)." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
