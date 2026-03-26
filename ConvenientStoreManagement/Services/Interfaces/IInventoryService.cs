using System.Collections.Generic;
using System.Threading.Tasks;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.ViewModels;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IInventoryService
    {
        Task CreateReceiptAsync(int userId, InventoryImportRequest request);

        Task<List<InventoryReceipt>> GetAllReceiptsAsync();

        Task<(InventoryReceipt? Receipt, List<InventoryReceiptDetail> Details)>
            GetReceiptDetailsAsync(int receiptId);
    }
}
