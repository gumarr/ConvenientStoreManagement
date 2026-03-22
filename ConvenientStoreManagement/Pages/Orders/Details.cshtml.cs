using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Orders
{
    [Authorize(Roles = "Admin,Staff")]
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;

        public DetailsModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Order? Order { get; set; }
        public List<OrderDetailViewModel> OrderItems { get; set; } = new List<OrderDetailViewModel>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _orderService.GetOrderDetailsAsync(id);
            if (Order == null)
            {
                return NotFound();
            }

            OrderItems = await _orderService.GetOrderItemsAsync(id);
            return Page();
        }
        
        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, string status)
        {
            await _orderService.UpdateOrderStatusAsync(orderId, status);
            TempData["SuccessMessage"] = $"Order status updated to {status}.";
            return RedirectToPage(new { id = orderId });
        }
    }
}
