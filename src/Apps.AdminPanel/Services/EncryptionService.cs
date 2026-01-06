using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Apps.AdminPanel.Models; // استدعاء المودل

namespace Apps.AdminPanel.Services
{
    // 1. تعريف واجهة (Interface) لضمان سهولة الاختبار والتطوير مستقبلاً
    public interface IEncryptionService
    {
        Task EncryptSingleFileAsync(DrmFile file, IProgress<int> progress);
        Task EncryptBatchFilesAsync(List<DrmFile> files, IProgress<int> progress);
    }

    // 2. تنفيذ الخدمة (هنا تضع كود التشفير الحقيقي لاحقاً)
    public class EncryptionService : IEncryptionService
    {
        public async Task EncryptSingleFileAsync(DrmFile file, IProgress<int> progress)
        {
            if (!File.Exists(file.Path)) throw new FileNotFoundException("File not found", file.Path);

            string outputFilePath = file.Path + ".enc";
            string keyFilePath = file.Path + ".key";

            await Task.Run(() =>
            {
                using (var engine = new DRM.Shared.Security.SecureVideoEngine())
                {
                    // 1. Generate Key (32 bytes) & IV (16 bytes) using Native Engine
                    byte[] key = engine.GenerateRandomData(32);
                    byte[] iv = engine.GenerateRandomData(16);

                    // 2. Initialize Engine
                    engine.Initialize(key, iv);

                    // 3. Save Key/IV to sidecar file (Temporary for verification)
                    // We save as Base64 strings to easily read them in SecurePlayer
                    string keyData = $"{Convert.ToBase64String(key)}|{Convert.ToBase64String(iv)}";
                    File.WriteAllText(keyFilePath, keyData);

                    // 4. Encrypt File
                    using (var fsInput = new FileStream(file.Path, FileMode.Open, FileAccess.Read))
                    using (var fsOutput = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                    {
                        long totalBytes = fsInput.Length;
                        long processedBytes = 0;
                        byte[] buffer = new byte[1024 * 1024]; // 1MB chunks
                        int bytesRead;

                        while ((bytesRead = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // Important: Use correct file offset for simulated random access (though for encryption linear is fine)
                            // Ideally offset should be the position in the file.
                            long fileOffset = fsInput.Position - bytesRead;
                            
                            // Process in-place (encrypts buffer)
                            engine.ProcessChunk(buffer, bytesRead, fileOffset);

                            fsOutput.Write(buffer, 0, bytesRead);

                            processedBytes += bytesRead;
                            int percent = (int)((double)processedBytes / totalBytes * 100);
                            progress?.Report(percent);
                        }
                    }
                }
            });
        }

        public async Task EncryptBatchFilesAsync(List<DrmFile> files, IProgress<int> progress)
        {
            int total = files.Count;
            for (int i = 0; i < total; i++)
            {
                await EncryptSingleFileAsync(files[i], null); // Pass null progress for individual file
                
                int percent = (int)(((double)(i + 1) / total) * 100);
                progress?.Report(percent);
            }
        }
    }
}
