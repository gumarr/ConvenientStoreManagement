using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    /// <summary>
    /// Member Card model - Thẻ thành viên tích điểm
    /// Không liên kết trực tiếp với User vì khách hàng không tài khoản
    /// Nhân viên nhập SĐT khách hàng để tích điểm
    /// </summary>
    public class MemberCard
    {
        [Key]
        public int MemberCardId { get; set; }

        /// <summary>
        /// Số điện thoại khách hàng - dùng để tìm kiếm
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        [Required]
        [StringLength(150)]
        public string FullName { get; set; }

        /// <summary>
        /// Email khách hàng (tùy chọn)
        /// </summary>
        [StringLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// Số điểm tích lũy hiện tại
        /// Được cộng khi thanh toán: TotalAmount * 0.01 (1% giá trị đơn)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LoyaltyPoints { get; set; } = 0;

        /// <summary>
        /// Ngày tạo thẻ thành viên
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Ngày cập nhật lần cuối
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
