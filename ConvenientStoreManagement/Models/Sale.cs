using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    public class Sale
    {
        [Key]
        public int SaleId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal DiscountAmount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } // Cash, Card, Transfer, etc.

        [StringLength(500)]
        public string Notes { get; set; }

        public bool IsCompleted { get; set; } = true;

        // Foreign key
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        // Navigation property
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
