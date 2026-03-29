using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConvenientStoreManagement.Services.Implementations
{
    public class MemberCardService : IMemberCardService
    {
        private readonly StoreDbContext _context;

        public MemberCardService(StoreDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tìm kiếm thẻ thành viên theo số điện thoại
        /// </summary>
        public async Task<MemberCard> GetMemberByPhoneAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            return await _context.MemberCards
                .FirstOrDefaultAsync(m => m.PhoneNumber == phoneNumber.Trim());
        }

        /// <summary>
        /// Tạo thẻ thành viên mới
        /// </summary>
        public async Task<MemberCard> CreateMemberAsync(string fullName, string phoneNumber, string? email = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber))
                    return null;

                // Kiểm tra xem số điện thoại đã tồn tại chưa
                var existingMember = await GetMemberByPhoneAsync(phoneNumber);
                if (existingMember != null)
                    return null; // Số điện thoại đã được sử dụng

                var memberCard = new MemberCard
                {
                    PhoneNumber = phoneNumber.Trim(),
                    FullName = fullName.Trim(),
                    Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                    LoyaltyPoints = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.MemberCards.Add(memberCard);
                await _context.SaveChangesAsync();

                return memberCard;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Cộng điểm tích lũy vào thẻ thành viên
        /// </summary>
        public async Task<bool> AddPointsAsync(int memberCardId, decimal points)
        {
            try
            {
                if (points <= 0)
                    return false;

                var memberCard = await _context.MemberCards.FindAsync(memberCardId);
                if (memberCard == null)
                    return false;

                memberCard.LoyaltyPoints += points;
                memberCard.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sử dụng/trừ điểm từ thẻ thành viên
        /// </summary>
        public async Task<bool> UsePointsAsync(int memberCardId, decimal points)
        {
            try
            {
                if (points <= 0)
                    return false;

                var memberCard = await _context.MemberCards.FindAsync(memberCardId);
                if (memberCard == null)
                    return false;

                // Kiểm tra điểm khả dụng
                if (memberCard.LoyaltyPoints < points)
                    return false;

                // Trừ điểm
                memberCard.LoyaltyPoints -= points;
                memberCard.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy tất cả thẻ thành viên
        /// </summary>
        public async Task<List<MemberCard>> GetAllMembersAsync()
        {
            return await _context.MemberCards
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}
