using System.Threading.Tasks;
using ConvenientStoreManagement.ViewModels;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IOrderService
    {
        Task<bool> ProcessCheckoutAsync(CheckoutViewModel model, int cashierUserId);
    }
}
