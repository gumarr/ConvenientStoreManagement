using System.Threading.Tasks;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IProductService
    {
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task UpdateStockAsync(int productId, int quantityToAdd);

        // Browse page methods
        Task<List<Product>> GetProductsAsync(int? categoryId, string? searchString, int pageIndex, int pageSize);
        Task<int> GetTotalCountAsync(int? categoryId, string? searchString);
        Task<List<Category>> GetCategoriesAsync();
    }
}
