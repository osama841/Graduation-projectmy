namespace Apps.AdminPanel.Models
{
    public class Asset
    {
        public int Id { get; set; }

        public string Title { get; set; } // عنوان الدرس
        public string FileName { get; set; } // اسم الملف المشفر (مثلاً: lesson1.enc)
        public string EncryptedFilePath { get; set; } // المسار الكامل للملف المشفر على القرص
        public long FileSize { get; set; } // الحجم بالبايت
        public string AssetType { get; set; } // "Video" or "PDF"

        // حقل JSON للمعلومات الإضافية (هنا فكرتك ممتازة)
        // نخزن فيه: {"Duration": "10:00", "Resolution": "1080p", "Author": "..."}
        public string MetaDataJson { get; set; }

        // الربط مع الكورس
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // الربط مع جدول الحماية (One-to-One)
        public AssetSecurity Security { get; set; }
    }
}