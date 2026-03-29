using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public bool Status { get; set; }

        /// <summary>
        /// Selling price = AverageImportPrice * PriceMultiplier.
        /// Default 1.5; must be > 0.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price multiplier must be greater than 0.")]
        public decimal PriceMultiplier { get; set; } = 1.5m;

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }

        public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
        public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetails { get; set; } = new List<InventoryReceiptDetail>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
