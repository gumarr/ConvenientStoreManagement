using System.Threading.Tasks;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConvenientStoreManagement.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;

        public DetailsModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Order Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _orderService.GetOrderByIdAsync(id);

            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
