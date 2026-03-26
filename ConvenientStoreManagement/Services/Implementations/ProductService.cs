using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly StoreDbContext _context;

        public ProductService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateStockAsync(int productId, int quantityToAdd)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Stock += quantityToAdd;
                await _context.SaveChangesAsync();
            }
        }

        private IQueryable<Product> BuildQuery(int? categoryId, string? searchString)
        {
            var query = _context.Products
                                .Include(p => p.Category)
                                .Include(p => p.ProductPrices)
                                .AsNoTracking()
                                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowerSearch = searchString.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearch));
            }

            return query;
        }

        public async Task<int> GetTotalCountAsync(int? categoryId, string? searchString)
        {
            var query = BuildQuery(categoryId, searchString);
            return await query.CountAsync();
        }

        public async Task<System.Collections.Generic.List<Product>> GetProductsAsync(int? categoryId, string? searchString, int pageIndex, int pageSize)
        {
            var query = BuildQuery(categoryId, searchString);

            return await query.OrderBy(p => p.Name)
                              .Skip((pageIndex - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
        }

        public async Task<System.Collections.Generic.List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                                 .OrderBy(c => c.Name)
                                 .AsNoTracking()
                                 .ToListAsync();
        }
    }
}
