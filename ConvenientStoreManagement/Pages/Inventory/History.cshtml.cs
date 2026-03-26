using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConvenientStoreManagement.Pages.Inventory
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly IInventoryService _inventoryService;

        public HistoryModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public List<InventoryReceipt> Receipts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Receipts = await _inventoryService.GetAllReceiptsAsync();
        }
    }
}
