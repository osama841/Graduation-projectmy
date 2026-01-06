namespace Apps.AdminPanel.Models
{
    public class ActiveUser
    {
        public string UserName { get; set; }
        public string LicenseStatus { get; set; } = "ساري"; // ساري، منتهي، ملغى
        public DateTime ExpiryDate { get; set; }
    }
}
