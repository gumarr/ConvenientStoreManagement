using System.ComponentModel.DataAnnotations;

namespace ConvenientStoreManagement.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Price { get; set; }
        
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
