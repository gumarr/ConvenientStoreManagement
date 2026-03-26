using System.Collections.Generic;
using System.Threading.Tasks;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConvenientStoreManagement.Pages.Orders
{
    public class HistoryModel : PageModel
    {
        private readonly IOrderService _orderService;

        public HistoryModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IList<Order> Orders { get; set; } = new List<Order>();

        public async Task OnGetAsync()
        {
            Orders = await _orderService.GetAllOrdersAsync();
        }
    }
}
