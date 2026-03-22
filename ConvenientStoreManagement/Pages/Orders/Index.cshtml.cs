using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace ConvenientStoreManagement.Pages.Orders
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;

        public IndexModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public PaginatedList<OrderViewModel> Orders { get; set; } = default!;

        public DateTime? CurrentStartDate { get; set; }
        public DateTime? CurrentEndDate { get; set; }
        public string? CurrentStatus { get; set; }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate, string? status, int? pageIndex)
        {
            CurrentStartDate = startDate;
            CurrentEndDate = endDate;
            CurrentStatus = status;

            int pageSize = 10;
            int pageNumber = pageIndex ?? 1;

            Orders = await _orderService.GetOrdersAsync(startDate, endDate, status, pageNumber, pageSize);
        }
    }
}
