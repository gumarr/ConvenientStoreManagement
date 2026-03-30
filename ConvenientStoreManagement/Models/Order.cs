using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// ID của thẻ thành viên (nếu khách hàng sử dụng)
        /// Nullable - nếu null = khách vãng lai không tích điểm
        /// </summary>
        public int? MemberCardId { get; set; }

        /// <summary>
        /// Số điểm tích lũy được từ đơn hàng này (1% TotalAmount trước khi trừ điểm)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LoyaltyPointsEarned { get; set; } = 0;

        /// <summary>
        /// Số điểm khách hàng sử dụng để trừ vào hóa đơn (tỉ lệ 1:1 với VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LoyaltyPointsUsed { get; set; } = 0;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
