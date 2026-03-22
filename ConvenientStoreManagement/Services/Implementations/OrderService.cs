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

        public OrderService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<OrderViewModel>> GetOrdersAsync(DateTime? startDate, DateTime? endDate, string? status, int pageIndex, int pageSize)
        {
            var query = _context.Orders.Include(o => o.User).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date <= endDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            query = query.OrderByDescending(o => o.CreatedAt);

            var viewModelQuery = query.Select(o => new OrderViewModel
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                CustomerName = o.User != null ? o.User.FullName : "Unknown",
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Status = o.Status
            });

            return await PaginatedList<OrderViewModel>.CreateAsync(viewModelQuery.AsNoTracking(), pageIndex, pageSize);
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<OrderDetailViewModel>> GetOrderItemsAsync(int orderId)
        {
            return await _context.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderId == orderId)
                .Select(od => new OrderDetailViewModel
                {
                    OrderDetailId = od.OrderDetailId,
                    ProductId = od.ProductId,
                    ProductName = od.Product != null ? od.Product.Name : "Unknown",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    DiscountPercent = od.DiscountPercent
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
