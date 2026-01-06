//using System.IO;
//using System.Threading.Tasks;
//using DRM.Shared.Security; // استدعاء مكتبتك المشتركة

//namespace Apps.AdminPanel.Services
//{
//    public class EncryptionService
//    {
//        // هذه الدالة هي المحرك الذي سيقوم بالعمل الثقيل
//        public void EncryptVideoFile(string inputPath, string outputPath, byte[] key, byte[] iv)
//        {
//            // 1. استدعاء الأداة من Shared
//            using (var engine = new SecureVideoEngine())
//            {
//                // تهيئة المحرك بالمفاتيح القادمة
//                engine.Initialize(key, iv);
//                // 2. فتح الملفات
//                using (var fsIn = new FileStream(inputPath, FileMode.Open))
//                using (var fsOut = new FileStream(outputPath, FileMode.Create))
//                {
//                    byte[] buffer = new byte[1024 * 1024]; // مخزن 1ميجا بايت
//                    int read;
              
//                    // 3. حلقة القراءة والتشفير والكتابة
//                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
//                    {
//                        long pos = fsIn.Position - read;
//                        // التشفير يتم هنا عبر C++
//                        engine.ProcessChunk(buffer, read, pos);

//                        // الكتابة للقرص
//                        fsOut.Write(buffer, 0, read);
//                    }
//                }
//            }
//        }
//    }
//}