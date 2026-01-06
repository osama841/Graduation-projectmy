namespace Apps.AdminPanel.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } // اسم الكورس
        public string Description { get; set; }
        public string CoverImagePath { get; set; } // مسار الصورة
        public bool IsPublished { get; set; }

        // علاقة: الكورس يحتوي على عدة ملفات
        public List<Asset> Assets { get; set; } = new List<Asset>();
    }
}