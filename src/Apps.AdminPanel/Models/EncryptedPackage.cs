using System;
using System.Collections.ObjectModel;

namespace Apps.AdminPanel.Models
{
    // تحديد نوع العنصر: هل هو حزمة (مجلد) أم ملف داخل الحزمة
    public enum ItemType { Package, File }

    public class EncryptedPackage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public DateTime DateCreated { get; set; }
        public int FileCount { get; set; }

        // =========================================================
        // 1. خصائص التنقل (لجعلها تعمل كمدير ملفات)
        // =========================================================
        public ItemType Type { get; set; } // هل هو مجلد أم ملف؟

        // محتويات المجلد (الأبناء)
        public ObservableCollection<EncryptedPackage> Children { get; set; }

        // =========================================================
        // 2. خصائص التراخيص (التي كانت مفقودة)
        // =========================================================
        // قائمة المستخدمين الذين يملكون صلاحية لهذه الحزمة
        public ObservableCollection<ActiveUser> ActiveUsers { get; set; }

        // الكونستركتور (تهيئة القوائم لتجنب الأخطاء)
        public EncryptedPackage()
        {
            Children = new ObservableCollection<EncryptedPackage>();
            ActiveUsers = new ObservableCollection<ActiveUser>(); // تمت إعادتها
            Type = ItemType.Package; // الافتراضي مجلد
        }
    }
}
