using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly StoreDbContext _context;
        private readonly IPricingService _pricingService;

        public OrderService(StoreDbContext context, IPricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<bool> ProcessCheckoutAsync(CheckoutViewModel model, int cashierUserId)
        {
            if (model == null || model.Items == null || model.Items.Count == 0) return false;

            // Start an Entity Framework Core Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Insert into Orders table
                var order = new Order
                {
                    UserId = cashierUserId,
                    OrderDate = DateTime.Now,
                    TotalAmount = model.TotalAmount
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); 

                DateTime today = order.OrderDate.Date;

                // Prepare Summary Stats
                var summaryStats = await _context.DailySummaryStats
                    .FirstOrDefaultAsync(s => s.Date == today);

                if (summaryStats == null)
                {
                    summaryStats = new DailySummaryStats 
                    { 
                        Date = today,
                        CreatedBy = cashierUserId,
                        Source = "Auto"
                    };
                    _context.DailySummaryStats.Add(summaryStats);
                }

                // 2. Insert into OrderDetails, Update Stock, and Update Stats
                foreach (var item in model.Items)
                {
                    // Look up current average import price
                    decimal currentImportPrice = await _pricingService.GetCurrentImportPriceAsync(item.ProductId);

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountApplied = item.DiscountApplied,
                        ImportCostSnapshot = currentImportPrice
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Stats calculation
                    decimal lineRevenue = (item.UnitPrice * item.Quantity) - item.DiscountApplied;
                    decimal lineCost = currentImportPrice * item.Quantity;
                    decimal lineProfit = lineRevenue - lineCost;

                    // Update DailyProductStats
                    var productStats = await _context.DailyProductStats
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId && p.Date == today);

                    if (productStats == null)
                    {
                        productStats = new DailyProductStats
                        {
                            ProductId = item.ProductId,
                            Date = today
                        };
                        _context.DailyProductStats.Add(productStats);
                    }

                    productStats.TotalQuantity += item.Quantity;
                    productStats.TotalRevenue += lineRevenue;
                    productStats.TotalImportCost += lineCost;
                    productStats.TotalProfit += lineProfit;

                    // Update DailySummaryStats
                    summaryStats.TotalRevenue += lineRevenue;
                    summaryStats.TotalImportCost += lineCost;
                    summaryStats.TotalProfit += lineProfit;

                    // 3. Update Product Stock
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        if (product.Stock < item.Quantity)
                        {
                            throw new Exception($"Sản phẩm '{product.Name}' không đủ tồn kho (Còn: {product.Stock}, Yêu cầu: {item.Quantity}).");
                        }
                        product.Stock -= item.Quantity;
                        _context.Products.Update(product);
                    }
                    else
                    {
                        throw new Exception($"Không tìm thấy sản phẩm với ID: {item.ProductId}");
                    }
                }

                await _context.SaveChangesAsync();

                // 4. Commit transaction
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Rollback on any error
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi thanh toán: {ex.Message}");
            }
        }
    }
}
