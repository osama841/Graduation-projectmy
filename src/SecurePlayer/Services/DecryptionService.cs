using System;
using System.IO;
using System.Threading.Tasks;
using DRM.Shared.Security;

namespace SecurePlayer.Services
{
    public class DecryptionService
    {
        private readonly SecureVideoEngine _engine;

        public DecryptionService()
        {
            _engine = new SecureVideoEngine();
        }

        /// <summary>
        /// Decrypts a file chunk and returns the decrypted bytes.
        /// </summary>
        /// <param name="encryptedData">The encrypted chunk data.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="iv">The initialization vector.</param>
        /// <param name="fileOffset">The offset in the file (for counter mode if used).</param>
        /// <returns>Decrypted byte array.</returns>
        public byte[] DecryptChunk(byte[] encryptedData, byte[] key, byte[] iv, long fileOffset)
        {
            try
            {
                // Initialize the engine with the key and IV
                // Note: In a real streaming scenario, Initialize might be called once per file, 
                // but here we ensure it's set up for the chunk.
                _engine.Initialize(key, iv);

                // Clone data to avoid modifying the original array in place if needed, 
                // but SecureVideoEngine usually modifies in-place or uses a buffer.
                // Assuming ProcessChunk encrypts/decrypts in-place.
                byte[] buffer = new byte[encryptedData.Length];
                Array.Copy(encryptedData, buffer, encryptedData.Length);

                _engine.ProcessChunk(buffer, buffer.Length, fileOffset);

                return buffer;
            }
            catch (Exception ex)
            {
                // Log exception here
                throw new Exception($"Decryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Decrypts a full file from path.
        /// Warning: This loads the full file into memory. Use only for small files like configs/licenses.
        /// </summary>
        public byte[] DecryptFile(string filePath, byte[] key, byte[] iv)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Encrypted file not found", filePath);

            byte[] fileBytes = File.ReadAllBytes(filePath);
            return DecryptChunk(fileBytes, key, iv, 0);
        }

        public void DecryptFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            var decryptedBytes = DecryptFile(inputFile, key, iv);
            File.WriteAllBytes(outputFile, decryptedBytes);
        }

         /// <summary>
        /// Initialize the decryption engine for a session.
        /// </summary>
        public void InitializeEngine(byte[] key, byte[] iv)
        {
             _engine.Initialize(key, iv);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
