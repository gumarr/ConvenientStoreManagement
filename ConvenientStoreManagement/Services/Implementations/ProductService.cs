using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly StoreDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductService(StoreDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private async Task<string?> ProcessUploadedFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/products/" + uniqueFileName;
        }

        public async Task<PaginatedList<ProductViewModel>> GetProductsAsync(string? search, int? categoryId, string? sortOrder, int pageIndex, int pageSize)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            query = sortOrder switch
            {
                "price_desc" => query.OrderByDescending(p => p.Price),
                "price_asc" => query.OrderBy(p => p.Price),
                _ => query.OrderBy(p => p.Name),
            };

            var viewModelQuery = query.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Unknown",
                Status = p.Status,
                ImageUrl = p.ImageUrl
            });

            return await PaginatedList<ProductViewModel>.CreateAsync(viewModelQuery.AsNoTracking(), pageIndex, pageSize);
        }

        public async Task<ProductInputModel?> GetProductForEditAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            return new ProductInputModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                Status = product.Status
            };
        }

        public async Task<bool> CreateProductAsync(ProductInputModel input)
        {
            if (input.ImageFile != null)
            {
                input.ImageUrl = await ProcessUploadedFileAsync(input.ImageFile);
            }

            var product = new Product
            {
                Name = input.Name,
                Price = input.Price,
                Stock = input.Stock,
                CategoryId = input.CategoryId,
                ImageUrl = input.ImageUrl,
                Status = input.Status
            };

            _context.Products.Add(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateProductAsync(ProductInputModel input)
        {
            var product = await _context.Products.FindAsync(input.ProductId);
            if (product == null) return false;

            if (input.ImageFile != null)
            {
                string? newImageUrl = await ProcessUploadedFileAsync(input.ImageFile);
                if (newImageUrl != null)
                {
                    product.ImageUrl = newImageUrl;
                }
            }
            else if (input.ImageUrl == null) 
            {
                // Optionally clear image if ImageUrl was explicitly set to null from UI
                // For now, if no new file is uploaded, keep existing image unless explicitly told otherwise.
            }

            product.Name = input.Name;
            product.Price = input.Price;
            product.Stock = input.Stock;
            product.CategoryId = input.CategoryId;
            product.Status = input.Status;

            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(e => e.ProductId == id);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
