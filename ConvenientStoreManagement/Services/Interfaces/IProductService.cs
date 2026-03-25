using System.Threading.Tasks;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IProductService
    {
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task UpdateStockAsync(int productId, int quantityToAdd);
    }
}
