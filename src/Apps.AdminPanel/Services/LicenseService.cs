using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apps.AdminPanel.Models;

namespace Apps.AdminPanel.Services
{
    public interface ILicenseService
    {
        Task<List<EncryptedPackage>> GetPackagesAsync();
        Task GrantLicenseAsync(string packageId, string userName, DateTime expiry);
    }

    public class LicenseService : ILicenseService
    {
        // دالة لجلب الحزم (محاكاة قاعدة البيانات)
        public async Task<List<EncryptedPackage>> GetPackagesAsync()
        {

            await Task.Delay(500); // محاكاة تأخير الشبكة

            var list = new List<EncryptedPackage>();                       // في دالة GetPackagesAsync داخل Service
            var folder1 = new EncryptedPackage
            {
                Name = "كورس البرمجة",
                Type = ItemType.Package, // هذا مجلد
                FileCount = 2
            };

            // إضافة ملفات داخل المجلد
            folder1.Children.Add(new EncryptedPackage { Name = "الدرس الأول.mp4", Type = ItemType.File, Size = "50 MB" });
            folder1.Children.Add(new EncryptedPackage { Name = "الدرس الثاني.mp4", Type = ItemType.File, Size = "120 MB" });

            list.Add(folder1); // إضافة المجلد للقائمة الرئيسية



            // بيانات وهمية للتجربة
            var pkg1 = new EncryptedPackage
            {
                Id = "1",
                Name = "كورس الرياضيات المتقدمة",
                FileCount = 15,
                Size = "450 MB",
                DateCreated = DateTime.Now.AddDays(-2)
            };
            pkg1.ActiveUsers.Add(new ActiveUser { UserName = "أحمد محمد", ExpiryDate = DateTime.Now.AddMonths(1) });
            pkg1.ActiveUsers.Add(new ActiveUser { UserName = "سارة علي", ExpiryDate = DateTime.Now.AddMonths(2) });

            var pkg2 = new EncryptedPackage
            {
                Id = "2",
                Name = "مستندات إدارية سرية 2025",
                FileCount = 4,
                Size = "12 MB",
                DateCreated = DateTime.Now.AddDays(-5)
            };

            list.Add(pkg1);
            list.Add(pkg2);
            list.Add(new EncryptedPackage { Id = "3", Name = "فيديوهات التدريب الصيفي", FileCount = 8, Size = "1.2 GB", DateCreated = DateTime.Now });

            return list;
        }

        // دالة لمنح الترخيص (مستقبلاً تتصل بـ Server أو تنشئ ملف ترخيص)
        public async Task GrantLicenseAsync(string packageId, string userName, DateTime expiry)
        {
            await Task.Delay(1000); // محاكاة المعالجة
            // هنا يتم كود إنشاء التشفير الحقيقي للمفتاح الخاص بالمستخدم
        }
    }
}
