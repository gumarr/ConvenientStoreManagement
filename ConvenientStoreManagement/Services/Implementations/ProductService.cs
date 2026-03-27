using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
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

        // ── Existing methods ──────────────────────────────────────────────────

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
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lowerSearch = searchString.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearch));
            }

            return query;
        }

        public async Task<int> GetTotalCountAsync(int? categoryId, string? searchString)
        {
            return await BuildQuery(categoryId, searchString).CountAsync();
        }

        public async Task<System.Collections.Generic.List<Product>> GetProductsAsync(
            int? categoryId, string? searchString, int pageIndex, int pageSize)
        {
            return await BuildQuery(categoryId, searchString)
                              .OrderBy(p => p.Name)
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

        // ── New: Edit helper ──────────────────────────────────────────────────

        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        // ── New: Import price & calculation ──────────────────────────────────

        /// <summary>
        /// Returns the ImportPrice from the most-recent InventoryReceiptDetail for the product.
        /// Returns 0 when no import records exist.
        /// </summary>
        public async Task<decimal> GetLatestImportPriceAsync(int productId)
        {
            var latestImport = await _context.InventoryReceiptDetails
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.ReceiptId)
                .FirstOrDefaultAsync();

            return latestImport?.ImportPrice ?? 0m;
        }

        /// <summary>Pure calculation – matches the same formula used on the frontend JS.</summary>
        public decimal CalculateSellingPrice(decimal importPrice, decimal multiplier)
            => importPrice * multiplier;

        // ── New: Product Detail ───────────────────────────────────────────────

        public async Task<ProductDetailDto?> GetProductDetailAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product is null)
                return null;

            // Latest import price (by ReceiptId desc)
            var avgImportPrice = await GetLatestImportPriceAsync(productId);

            // Active selling price (EndDate == null)
            var activePrice = await _context.ProductPrices
                .Where(p => p.ProductId == productId && p.EndDate == null)
                .FirstOrDefaultAsync();

            return new ProductDetailDto
            {
                Product           = product,
                AvgImportPrice    = avgImportPrice,
                CurrentSellingPrice = activePrice?.Price
            };
        }
    }
}
