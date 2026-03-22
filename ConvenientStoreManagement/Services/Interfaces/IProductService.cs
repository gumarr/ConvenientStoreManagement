using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.ViewModels;
using System.Threading.Tasks;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IProductService
    {
        Task<PaginatedList<ProductViewModel>> GetProductsAsync(string? search, int? categoryId, string? sortOrder, int pageIndex, int pageSize);
        Task<ProductInputModel?> GetProductForEditAsync(int id);
        Task<bool> CreateProductAsync(ProductInputModel input);
        Task<bool> UpdateProductAsync(ProductInputModel input);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
