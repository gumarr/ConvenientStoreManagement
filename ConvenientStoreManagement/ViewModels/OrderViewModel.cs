using System.ComponentModel.DataAnnotations;

namespace ConvenientStoreManagement.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal TotalAmount { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class OrderDetailViewModel
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; }
        
        public int DiscountPercent { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total => (UnitPrice * Quantity) * (1 - (DiscountPercent / 100m));
    }
}
