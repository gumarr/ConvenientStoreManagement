using System.Threading.Tasks;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace ConvenientStoreManagement.Pages.Inventory
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IInventoryService _inventoryService;

        public DetailsModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public InventoryReceipt? Receipt { get; set; }
        public List<InventoryReceiptDetail> Details { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var (receipt, details) = await _inventoryService.GetReceiptDetailsAsync(id);
            if (receipt == null) return NotFound();

            Receipt = receipt;
            Details = details;
            return Page();
        }
    }
}
