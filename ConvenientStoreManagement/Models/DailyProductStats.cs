using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    /// <summary>
    /// Per-product per-day revenue and profit snapshot.
    /// Updated atomically inside the Order transaction.
    /// </summary>
    public class DailyProductStats
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        /// <summary>Stored as a plain DateTime with time truncated to midnight.</summary>
        [Required]
        public DateTime Date { get; set; }

        public int TotalQuantity { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalRevenue { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalImportCost { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalProfit { get; set; } = 0;

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}
