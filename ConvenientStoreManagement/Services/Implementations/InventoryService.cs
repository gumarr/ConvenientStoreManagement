using System;
using System.Linq;
using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly StoreDbContext _context;
        private readonly IProductService _productService;
        private readonly IPricingService _pricingService;

        public InventoryService(StoreDbContext context, IProductService productService, IPricingService pricingService)
        {
            _context = context;
            _productService = productService;
            _pricingService = pricingService;
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
                        // Existing Product
                        var product = await _productService.GetProductByIdAsync(item.ProductId.Value);
                        if (product == null) throw new Exception($"Product {item.ProductId} not found.");

                        int oldStock = product.Stock;
                        decimal oldImportPrice = await _pricingService.GetCurrentImportPriceAsync(product.ProductId);

                        decimal newAvgPrice = 0;
                        if (oldStock + item.Quantity > 0)
                        {
                            newAvgPrice = ((oldStock * oldImportPrice) + (item.Quantity * item.ImportPrice)) / (oldStock + item.Quantity);
                        }

                        // Update Stock
                        product.Stock += item.Quantity;
                        _context.Products.Update(product);

                        // Insert Receipt Detail
                        var detail = new InventoryReceiptDetail
                        {
                            ReceiptId = receipt.ReceiptId,
                            ProductId = product.ProductId,
                            Quantity = item.Quantity,
                            OriginalImportPrice = item.ImportPrice, // Raw input price
                            ImportPrice = newAvgPrice // Use weighted average
                        };
                        _context.InventoryReceiptDetails.Add(detail);

                        // Update Selling Price
                        decimal sellingPrice = newAvgPrice * 1.5m;
                        await _pricingService.UpdateSellingPriceAsync(product.ProductId, sellingPrice);
                    }
                    else
                    {
                        // New Product
                        var product = new Product
                        {
                            Name = item.Name,
                            CategoryId = item.CategoryId.Value,
                            Unit = item.Unit,
                            Stock = item.Quantity,
                            Status = true,
                            ImageUrl = "" // Default or empty
                        };
                        await _productService.CreateProductAsync(product);

                        // Insert Receipt Detail
                        var detail = new InventoryReceiptDetail
                        {
                            ReceiptId = receipt.ReceiptId,
                            ProductId = product.ProductId,
                            Quantity = item.Quantity,
                            OriginalImportPrice = item.ImportPrice, // Raw input price
                            ImportPrice = item.ImportPrice // Same as Original for new product
                        };
                        _context.InventoryReceiptDetails.Add(detail);

                        // Update Selling Price
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
    }
}
