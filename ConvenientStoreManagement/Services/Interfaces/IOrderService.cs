using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.ViewModels;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IOrderService
    {
        Task<PaginatedList<OrderViewModel>> GetOrdersAsync(DateTime? startDate, DateTime? endDate, string? status, int pageIndex, int pageSize);
        Task<Order?> GetOrderDetailsAsync(int orderId);
        Task<List<OrderDetailViewModel>> GetOrderItemsAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    }
}
