using System.Collections.Generic;

namespace ConvenientStoreManagement.ViewModels
{
    public class CheckoutViewModel
    {
        /// <summary>
        /// ID của thẻ thành viên (tùy chọn)
        /// Nếu null = khách hàng không có tài khoản
        /// </summary>
        public int? MemberCardId { get; set; }

        /// <summary>
        /// Tên khách hàng (nếu không có thẻ thành viên)
        /// </summary>
        public string? MemberName { get; set; }

        /// <summary>
        /// Số điện thoại khách hàng (nếu không có thẻ thành viên)
        /// </summary>
        /// <summary>
        /// Số điểm khách hàng muốn sử dụng để trừ vào hóa đơn (1:1 với VNĐ)
        /// </summary>
        public decimal LoyaltyPointsToUse { get; set; } = 0;

        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        public decimal TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }
    }
}
