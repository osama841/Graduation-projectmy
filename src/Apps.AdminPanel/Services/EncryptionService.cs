using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Apps.AdminPanel.Models;

namespace Apps.AdminPanel.Services
{
    // الواجهة: تحدد العقد بين الخدمة والـ UI
    public interface IEncryptionService
    {
        Task EncryptSingleFileAsync(DrmFile file, IProgress<int> progress);
        Task EncryptBatchFilesAsync(List<DrmFile> files, IProgress<int> progress);
    }

    // تنفيذ الخدمة: تقوم بالتشفير الفعلي
    public class EncryptionService : IEncryptionService
    {
        /// <summary>
        /// تشفير ملف واحد باستخدام WhiteBox Cryptography
        /// ميزة WhiteBox: لا يوجد مفتاح ظاهر في الذاكرة أو الملفات
        /// </summary>
        /// <param name="file">الملف المراد تشفيره</param>
        /// <param name="progress">كائن لتتبع نسبة الإنجاز (0-100%)</param>
        public async Task EncryptSingleFileAsync(DrmFile file, IProgress<int> progress)
        {
            // 1. التحقق من وجود الملف قبل البدء
            if (!File.Exists(file.Path)) 
                throw new FileNotFoundException("الملف غير موجود", file.Path);

            // 2. تحديد مسار الملف المشفر (نضيف .enc في النهاية)
            string outputFilePath = file.Path + ".enc";

            // 3. تنفيذ التشفير في خيط منفصل (async) لعدم تجميد الـ UI
            await Task.Run(() =>
            {
                // 4. قراءة الملف بالكامل إلى الذاكرة
                // ملاحظة: للملفات الكبيرة جداً، يُفضّل القراءة chunk by chunk
                byte[] fileData = File.ReadAllBytes(file.Path);
                
                long totalBytes = fileData.Length;
                
                // 5. حجم الـ block للتشفير (WhiteBox AES يعمل على 16 bytes)
                int chunkSize = 16;
                
                // 6. فتح ملف الإخراج للكتابة
                using (var fsOutput = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                {
                    // 7. حلقة: نقسم الملف إلى chunks ونشفر كل واحد
                    for (int i = 0; i < fileData.Length; i += chunkSize)
                    {
                        // 8. حساب حجم الـ chunk الحالي
                        // (آخر chunk قد يكون أقل من 16 bytes)
                        int currentChunkSize = Math.Min(chunkSize, fileData.Length - i);
                        
                        // 9. نسخ الـ chunk من الملف الأصلي
                        byte[] chunk = new byte[currentChunkSize];
                        Array.Copy(fileData, i, chunk, 0, currentChunkSize);
                        
                        // ⭐ 10. التشفير باستخدام WhiteBox
                        // ملاحظة: لا نحتاج مفتاح! المفتاح مدفون في الجداول
                        byte[] encryptedChunk = DRM.Shared.Security.SecureVideoEngine.EncryptWithWhiteBox(chunk);
                        
                        // 11. كتابة الـ chunk المشفر إلى الملف
                        fsOutput.Write(encryptedChunk, 0, encryptedChunk.Length);
                        
                        // 12. حساب نسبة الإنجاز وإرسالها للـ UI
                        int percent = (int)(((double)(i + currentChunkSize) / totalBytes) * 100);
                        progress?.Report(percent);
                    }
                }
                
                // ✅ التشفير اكتمل!
                // لا يوجد ملف .key (المفتاح مخفي في الجداول)
            });
        }

        /// <summary>
        /// تشفير مجموعة من الملفات (Batch Processing)
        /// </summary>
        /// <param name="files">قائمة الملفات المراد تشفيرها</param>
        /// <param name="progress">نسبة إنجاز المجموعة الكاملة</param>
        public async Task EncryptBatchFilesAsync(List<DrmFile> files, IProgress<int> progress)
        {
            int total = files.Count;
            
            // حلقة: نشفر كل ملف على حدة
            for (int i = 0; i < total; i++)
            {
                // تشفير الملف (progress = null لأننا نتتبع الإجمالي فقط)
                await EncryptSingleFileAsync(files[i], null);
                
                // حساب نسبة الإنجاز للمجموعة
                int percent = (int)(((double)(i + 1) / total) * 100);
                progress?.Report(percent);
            }
        }
    }
}
