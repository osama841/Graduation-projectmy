using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.AdminPanel.Models
{
    public class User
    {
        [Key] // يعني أن هذا هو الرقم المميز (Primary Key)
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } // اسم الدخول

        [Required]
        public string PasswordHash { get; set; } // كلمة المرور المشفرة

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } // الاسم الكامل

        public string Email { get; set; }

        public string Role { get; set; } // Admin, User, Supervisor

        public string SecurityKey { get; set; } // المفتاح الأمني الخاص بالمستخدم

        public bool IsActive { get; set; } = true; // حالة الحساب

        public DateTime CreatedAt { get; set; } = DateTime.Now; // تاريخ الإنشاء
    }
}

