using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    public class InventoryReceipt
    {
        [Key]
        public int ReceiptId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ImportDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public virtual ICollection<InventoryReceiptDetail> ReceiptDetails { get; set; } = new List<InventoryReceiptDetail>();
    }
}
