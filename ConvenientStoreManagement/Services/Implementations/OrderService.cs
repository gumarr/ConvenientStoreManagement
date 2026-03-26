using System;
using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using ConvenientStoreManagement.ViewModels;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly StoreDbContext _context;

        public OrderService(StoreDbContext context)
        {
            _context = context;
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

                // 2. Insert into OrderDetails and Update Product Stock
                foreach (var item in model.Items)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountApplied = item.DiscountApplied
                    };
                    _context.OrderDetails.Add(orderDetail);

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
