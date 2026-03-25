using System.Threading.Tasks;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IPricingService
    {
        Task<decimal> GetCurrentImportPriceAsync(int productId);
        Task UpdateSellingPriceAsync(int productId, decimal newSellingPrice);
    }
}
