using System.Collections.Generic;

namespace ConvenientStoreManagement.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
    }
}
