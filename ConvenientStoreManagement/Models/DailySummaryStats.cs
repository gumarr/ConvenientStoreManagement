using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    /// <summary>
    /// System-wide daily revenue and profit summary.
    /// One row per calendar day; Date column has a unique index.
    /// </summary>
    public class DailySummaryStats
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Stored as a plain DateTime with time truncated to midnight.</summary>
        [Required]
        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalRevenue { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalImportCost { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalProfit { get; set; } = 0;

        public int? CreatedBy { get; set; }
        
        [ForeignKey("CreatedBy")]
        public User CreatedByUser { get; set; }

        [StringLength(50)]
        public string Source { get; set; }
    }
}
