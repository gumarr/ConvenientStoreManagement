using System.Collections.Generic;
using System.Threading.Tasks;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Services.Interfaces
{
    public interface IMemberCardService
    {
        /// <summary>
        /// Tìm kiếm thẻ thành viên theo số điện thoại
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại khách hàng</param>
        /// <returns>MemberCard nếu tìm thấy, null nếu không</returns>
        Task<MemberCard> GetMemberByPhoneAsync(string phoneNumber);

        /// <summary>
        /// Tạo thẻ thành viên mới
        /// </summary>
        /// <param name="fullName">Tên khách hàng</param>
        /// <param name="phoneNumber">Số điện thoại (unique)</param>
        /// <param name="email">Email (tùy chọn)</param>
        /// <returns>MemberCard vừa tạo, null nếu thất bại</returns>
        Task<MemberCard> CreateMemberAsync(string fullName, string phoneNumber, string? email = null);

        /// <summary>
        /// Cộng điểm tích lũy vào thẻ thành viên (1% giá trị đơn)
        /// </summary>
        /// <param name="memberCardId">ID của thẻ thành viên</param>
        /// <param name="points">Số điểm cần cộng</param>
        /// <returns>true nếu thành công, false nếu thất bại</returns>
        Task<bool> AddPointsAsync(int memberCardId, decimal points);

        /// <summary>
        /// Sử dụng/trừ điểm từ thẻ thành viên
        /// </summary>
        /// <param name="memberCardId">ID của thẻ thành viên</param>
        /// <param name="points">Số điểm cần trừ</param>
        /// <returns>true nếu thành công, false nếu không đủ điểm hoặc thất bại</returns>
        Task<bool> UsePointsAsync(int memberCardId, decimal points);

        /// <summary>
        /// Lấy tất cả thẻ thành viên
        /// </summary>
        Task<List<MemberCard>> GetAllMembersAsync();
    }
}
