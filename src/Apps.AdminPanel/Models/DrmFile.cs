namespace Apps.AdminPanel.Models
{
    public class DrmFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; } // الحجم الخام للحسابات
        public string SizeDisplay { get; set; } // الحجم للعرض (نص)
        public DateTime ModifiedDate { get; set; }
        public string Icon { get; set; }
        public bool IsEncrypted { get; set; }
    }
}
