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
        private readonly IMemberCardService _memberCardService;

        public OrderService(StoreDbContext context, IPricingService pricingService, IMemberCardService memberCardService)
        {
            _context = context;
            _pricingService = pricingService;
            _memberCardService = memberCardService;
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
                // Tính subtotal
                decimal subtotal = 0;
                foreach (var item in model.Items)
                {
                    subtotal += item.UnitPrice * item.Quantity;
                }

                // Tính điểm tích lũy (1% của subtotal trước khi trừ điểm)
                decimal loyaltyPointsEarned = Math.Round(subtotal * 0.01m, 2);

                // Kiểm tra và trừ điểm từ thẻ thành viên (nếu có)
                decimal loyaltyPointsUsed = 0;
                if (model.MemberCardId.HasValue && model.MemberCardId.Value > 0 && model.LoyaltyPointsToUse > 0)
                {
                    // Kiểm tra điểm khả dụng
                    var member = await _context.MemberCards.FindAsync(model.MemberCardId.Value);
                    if (member != null && member.LoyaltyPoints >= model.LoyaltyPointsToUse)
                    {
                        // Trừ điểm
                        bool pointsDeducted = await _memberCardService.UsePointsAsync(model.MemberCardId.Value, model.LoyaltyPointsToUse);
                        if (pointsDeducted)
                        {
                            loyaltyPointsUsed = model.LoyaltyPointsToUse;
                        }
                    }
                }

                // 1. Insert into Orders table
                var order = new Order
                {
                    UserId = cashierUserId,
                    OrderDate = DateTime.Now,
                    TotalAmount = model.TotalAmount,
                    MemberCardId = model.MemberCardId,
                    LoyaltyPointsEarned = loyaltyPointsEarned,
                    LoyaltyPointsUsed = loyaltyPointsUsed
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

                // Prepare for discount distribution
                decimal totalBeforeDiscount = subtotal;
                decimal totalDiscountAllowed = loyaltyPointsUsed;
                decimal remainingDiscount = totalDiscountAllowed;
                int count = 0;
                int totalItems = model.Items.Count;

                // 2. Insert into OrderDetails, Update Stock, and Update Stats
                foreach (var item in model.Items)
                {
                    count++;
                    decimal itemTotal = item.UnitPrice * item.Quantity;
                    
                    // Distribute discount proportionally
                    decimal itemDiscount = 0;
                    if (totalBeforeDiscount > 0 && totalDiscountAllowed > 0)
                    {
                        if (count == totalItems)
                        {
                            // Last item absorbs the remaining rounding difference
                            itemDiscount = remainingDiscount;
                        }
                        else
                        {
                            decimal ratio = itemTotal / totalBeforeDiscount;
                            itemDiscount = Math.Round(totalDiscountAllowed * ratio, 2);
                            remainingDiscount -= itemDiscount;
                        }
                    }
                    // Look up current average import price
                    decimal currentImportPrice = await _pricingService.GetCurrentImportPriceAsync(item.ProductId);

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountApplied = itemDiscount,
                        ImportCostSnapshot = currentImportPrice
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Stats calculation
                    decimal lineRevenue = (item.UnitPrice * item.Quantity) - itemDiscount;
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

                // 4. Cộng điểm tích lũy cho thẻ thành viên (1% giá trị subtotal)
                if (model.MemberCardId.HasValue && model.MemberCardId.Value > 0 && loyaltyPointsEarned > 0)
                {
                    await _memberCardService.AddPointsAsync(model.MemberCardId.Value, loyaltyPointsEarned);
                }

                // 5. Commit transaction
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
