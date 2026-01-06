namespace Apps.AdminPanel.Models
{
    public class AssetSecurity
    {
        public int Id { get; set; }

        // المفاتيح نخزنها كبيانات خام (BLOB)
        public byte[] MasterKey { get; set; } // مفتاح التشفير
        public byte[] IV { get; set; }        // متجه التهيئة

        public string EncryptionAlgo { get; set; } = "AES-256"; // نوع الخوارزمية

        public string Checksum { get; set; } // MD5 للملف الأصلي للتأكد من سلامته

        // الربط مع الملف (Asset)
        public int AssetId { get; set; }
        public Asset Asset { get; set; }
    }
}