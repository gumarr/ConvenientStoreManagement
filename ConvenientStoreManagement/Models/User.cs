using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConvenientStoreManagement.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        /// <summary>
        /// Nullable: user đăng ký bằng Google không có password.
        /// Nếu null → chỉ có thể login bằng Google.
        /// </summary>
        [StringLength(256)]
        public string? Password { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; }

        /// <summary>Email từ Google hoặc do user cung cấp.</summary>
        [StringLength(256)]
        public string? Email { get; set; }

        /// <summary>Google Subject ID — dùng để match tài khoản Google.</summary>
        [StringLength(256)]
        public string? GoogleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } // Admin / Staff

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
