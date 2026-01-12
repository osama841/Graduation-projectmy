using System;
using System.IO;
using System.Threading.Tasks;
using DRM.Shared.Security;

namespace SecurePlayer.Services
{
    /// <summary>
    /// خدمة فك التشفير - تستخدم WhiteBox Cryptography
    /// </summary>
    public class DecryptionService
    {
        private readonly SecureVideoEngine _engine;

        /// <summary>
        /// بناء الخدمة - ننشئ محرك التشفير
        /// </summary>
        public DecryptionService()
        {
            _engine = new SecureVideoEngine();
        }

        // ═══════════════════════════════════════════════════════
        // القسم 1: الدوال الجديدة (WhiteBox)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// فك تشفير chunk باستخدام WhiteBox (بدون مفتاح!)
        /// تُستخدم لفك تشفير أجزاء الفيديو أثناء التشغيل
        /// </summary>
        /// <param name="encryptedData">البيانات المشفرة</param>
        /// <returns>البيانات بعد فك التشفير</returns>
        public byte[] DecryptChunkWithWhiteBox(byte[] encryptedData)
        {
            try
            {
                // فك التشفير باستخدام WhiteBox
                // ملاحظة: لا نحتاج key أو iv!
                return SecureVideoEngine.DecryptWithWhiteBox(encryptedData);
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل فك التشفير WhiteBox: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// فك تشفير ملف كامل باستخدام WhiteBox
        /// يُستخدم لفك تشفير الفيديوهات المحفوظة
        /// </summary>
        /// <param name="inputFilePath">مسار الملف المشفر (.enc)</param>
        /// <param name="outputFilePath">مسار الملف بعد فك التشفير</param>
        public void DecryptFileWithWhiteBox(string inputFilePath, string outputFilePath)
        {
            // 1. التحقق من وجود الملف
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("الملف المشفر غير موجود", inputFilePath);

            // 2. قراءة الملف المشفر
            byte[] encryptedData = File.ReadAllBytes(inputFilePath);

            // 3. حساب حجم الـ chunks (16 bytes لكل block في AES)
            int chunkSize = 16;
            
            // 4. فتح ملف الإخراج للكتابة
            using (var fsOutput = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                // 5. حلقة: فك تشفير كل chunk على حدة
                for (int i = 0; i < encryptedData.Length; i += chunkSize)
                {
                    // حساب حجم الـ chunk الحالي
                    int currentChunkSize = Math.Min(chunkSize, encryptedData.Length - i);
                    
                    // نسخ الـ chunk
                    byte[] encryptedChunk = new byte[currentChunkSize];
                    Array.Copy(encryptedData, i, encryptedChunk, 0, currentChunkSize);
                    
                    // فك التشفير باستخدام WhiteBox
                    byte[] decryptedChunk = DecryptChunkWithWhiteBox(encryptedChunk);
                    
                    // كتابة النتيجة
                    fsOutput.Write(decryptedChunk, 0, decryptedChunk.Length);
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        // القسم 2: الدوال القديمة (AES عادي - للترخيص فقط)
        // نبقيها للتوافق مع الكود القديم
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// فك تشفير chunk باستخدام AES عادي (للترخيص فقط)
        /// هذه الدالة قديمة - تُستخدم فقط لملفات الترخيص
        /// </summary>
        /// <param name="encryptedData">البيانات المشفرة</param>
        /// <param name="key">المفتاح (32 bytes)</param>
        /// <param name="iv">الـ IV (16 bytes)</param>
        /// <param name="fileOffset">موقع الـ chunk في الملف</param>
        /// <returns>البيانات بعد فك التشفير</returns>
        public byte[] DecryptChunk(byte[] encryptedData, byte[] key, byte[] iv, long fileOffset)
        {
            try
            {
                // تهيئة المحرك بالمفتاح
                _engine.Initialize(key, iv);

                // نسخ البيانات (لأن ProcessChunk يعدّل in-place)
                byte[] buffer = new byte[encryptedData.Length];
                Array.Copy(encryptedData, buffer, encryptedData.Length);

                // فك التشفير
                _engine.ProcessChunk(buffer, buffer.Length, fileOffset);

                return buffer;
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل فك التشفير AES: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// فك تشفير ملف كامل باستخدام AES عادي (للترخيص فقط)
        /// هذه الدالة قديمة - تُستخدم فقط لملفات الترخيص
        /// </summary>
        public byte[] DecryptFile(string filePath, byte[] key, byte[] iv)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("الملف المشفر غير موجود", filePath);

            byte[] fileBytes = File.ReadAllBytes(filePath);
            return DecryptChunk(fileBytes, key, iv, 0);
        }

        /// <summary>
        /// فك تشفير ملف وحفظه (AES عادي - للترخيص فقط)
        /// </summary>
        public void DecryptFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            var decryptedBytes = DecryptFile(inputFile, key, iv);
            File.WriteAllBytes(outputFile, decryptedBytes);
        }

        /// <summary>
        /// تهيئة المحرك بالمفتاح (AES عادي - للترخيص فقط)
        /// </summary>
        public void InitializeEngine(byte[] key, byte[] iv)
        {
            _engine.Initialize(key, iv);
        }

        // ═══════════════════════════════════════════════════════
        // التنظيف
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// تحرير الموارد عند الانتهاء
        /// </summary>
        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
