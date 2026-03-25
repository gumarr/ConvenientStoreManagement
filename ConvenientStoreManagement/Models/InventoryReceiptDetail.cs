using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    public class InventoryReceiptDetail
    {
        [Key]
        public int ReceiptDetailId { get; set; }

        [Required]
        public int ReceiptId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ImportPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OriginalImportPrice { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        public virtual InventoryReceipt Receipt { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }
    }
}
