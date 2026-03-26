using System.Collections.Generic;
using System.Threading.Tasks;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.ViewModels;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IOrderService
    {
        Task<bool> ProcessCheckoutAsync(CheckoutViewModel model, int cashierUserId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int orderId);
    }
}
