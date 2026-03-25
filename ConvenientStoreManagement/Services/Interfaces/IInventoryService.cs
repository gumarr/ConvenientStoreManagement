using System.Threading.Tasks;
using ConvenientStoreManagement.ViewModels;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IInventoryService
    {
        Task CreateReceiptAsync(int userId, InventoryImportRequest request);
    }
}
