using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly StoreDbContext _context;
        private readonly IProductService _productService;
        private readonly IPricingService _pricingService;
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private const long MaxImageBytes = 3 * 1024 * 1024; // 3 MB

        public InventoryService(StoreDbContext context, IProductService productService,
            IPricingService pricingService, IWebHostEnvironment env)
        {
            _context = context;
            _productService = productService;
            _pricingService = pricingService;
            _env = env;
        }

        public async Task CreateReceiptAsync(int userId, InventoryImportRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalCost = request.Items.Sum(i => i.Quantity * i.ImportPrice);

                var receipt = new InventoryReceipt
                {
                    UserId = userId,
                    ImportDate = DateTime.Now,
                    TotalCost = totalCost
                };

                _context.InventoryReceipts.Add(receipt);
                await _context.SaveChangesAsync();

                foreach (var item in request.Items)
                {
                    if (item.ProductId.HasValue && item.ProductId.Value > 0)
                    {
                        // ── Existing Product ──────────────────────────────────────
                        var product = await _productService.GetProductByIdAsync(item.ProductId.Value);
                        if (product == null) throw new Exception($"Product {item.ProductId} not found.");

                        int oldStock = product.Stock;
                        decimal oldImportPrice = await _pricingService.GetCurrentImportPriceAsync(product.ProductId);

                        decimal newAvgPrice = 0;
                        if (oldStock + item.Quantity > 0)
                        {
                            newAvgPrice = ((oldStock * oldImportPrice) + (item.Quantity * item.ImportPrice))
                                          / (oldStock + item.Quantity);
                        }

                        product.Stock += item.Quantity;
                        _context.Products.Update(product);

                        var detail = new InventoryReceiptDetail
                        {
                            ReceiptId = receipt.ReceiptId,
                            ProductId = product.ProductId,
                            Quantity = item.Quantity,
                            OriginalImportPrice = item.ImportPrice,
                            ImportPrice = newAvgPrice
                        };
                        _context.InventoryReceiptDetails.Add(detail);

                        decimal sellingPrice = newAvgPrice * 1.5m;
                        await _pricingService.UpdateSellingPriceAsync(product.ProductId, sellingPrice);
                    }
                    else
                    {
                        // ── New Product ───────────────────────────────────────────
                        // Handle image upload
                        string imageUrl = string.Empty;
                        if (item.ImageFile != null && item.ImageFile.Length > 0)
                        {
                            imageUrl = await SaveProductImageAsync(item);
                        }

                        var product = new Product
                        {
                            Name = item.Name!,
                            CategoryId = item.CategoryId!.Value,
                            Unit = item.Unit,
                            Stock = item.Quantity,
                            Status = true,
                            ImageUrl = imageUrl
                        };
                        await _productService.CreateProductAsync(product);

                        var detail = new InventoryReceiptDetail
                        {
                            ReceiptId = receipt.ReceiptId,
                            ProductId = product.ProductId,
                            Quantity = item.Quantity,
                            OriginalImportPrice = item.ImportPrice,
                            ImportPrice = item.ImportPrice
                        };
                        _context.InventoryReceiptDetails.Add(detail);

                        decimal sellingPrice = item.ImportPrice * 1.5m;
                        await _pricingService.UpdateSellingPriceAsync(product.ProductId, sellingPrice);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ── History Methods ───────────────────────────────────────────────────────

        public async Task<List<InventoryReceipt>> GetAllReceiptsAsync()
        {
            return await _context.InventoryReceipts
                .Include(r => r.User)
                .OrderByDescending(r => r.ImportDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(InventoryReceipt? Receipt, List<InventoryReceiptDetail> Details)>
            GetReceiptDetailsAsync(int receiptId)
        {
            var receipt = await _context.InventoryReceipts
                .Include(r => r.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReceiptId == receiptId);

            if (receipt == null) return (null, new List<InventoryReceiptDetail>());

            var details = await _context.InventoryReceiptDetails
                .Include(d => d.Product)
                .Where(d => d.ReceiptId == receiptId)
                .AsNoTracking()
                .ToListAsync();

            return (receipt, details);
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Validates and saves the uploaded image file for a new product.
        /// Returns the relative URL path (e.g. "/img/Products/guid.jpg").
        /// Throws InvalidOperationException on validation failure.
        /// </summary>
        private async Task<string> SaveProductImageAsync(InventoryImportItem item)
        {
            var file = item.ImageFile!;

            // Size validation
            if (file.Length > MaxImageBytes)
                throw new InvalidOperationException(
                    $"Image for '{item.Name}' exceeds the 3 MB limit.");

            // MIME type validation (don't trust extension alone)
            if (!AllowedMimeTypes.Contains(file.ContentType))
                throw new InvalidOperationException(
                    $"Image for '{item.Name}' has an unsupported format. Allowed: jpg, jpeg, png, webp.");

            // Determine safe extension from MIME
            var ext = file.ContentType.ToLower() switch
            {
                "image/jpeg" => ".jpg",
                "image/png"  => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };

            var fileName = $"{Guid.NewGuid()}{ext}";
            var folderPath = Path.Combine(_env.WebRootPath, "img", "Products");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/img/Products/{fileName}";
        }
    }
}
