using System;
using System.ComponentModel.DataAnnotations;

namespace ConvenientStoreManagement.Models
{
    public class Sale
    {
        [Key]
        public int SaleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public int DiscountPercent { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }
    }
}
