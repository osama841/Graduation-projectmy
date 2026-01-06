using Apps.AdminPanel.DBconection;
using Apps.AdminPanel.Models;
namespace Apps.AdminPanel.ViewModels
{
    public static class DatabaseHelper
    {
        public static void InitializeDatabase()
        {
            using (var context = new AppDbContext())
            {
                // 1. تأكد أن قاعدة البيانات مخلوقة
                context.Database.EnsureCreated();

                // 2. إذا لم يكن هناك أي مستخدم، أضف مستخدم أدمن افتراضي
                if (!context.Users.Any())
                {
                    var admin = new User
                    {
                        FullName = "System Administrator",
                        Username = "admin",
                        Email = "admin@system.local",
                        // ملاحظة: كلمة المرور هنا "123456" لكننا سنضيف التشفير لاحقاً
                        // حالياً نضعها نص عادي للتجربة فقط
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                      
                        Role = "Admin",
                        SecurityKey = Guid.NewGuid().ToString("N").ToUpper(), // توليد مفتاح عشوائي
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    context.Users.Add(admin);
                    context.SaveChanges();
                }
            }
        }
    }
}
