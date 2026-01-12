using System;
using System.Runtime.InteropServices;

namespace DRM.Shared.Security
{
    public class SecureVideoEngine : IDisposable
    {
        // ═══════════════════════════════════════════════════════
        // القسم 1: الدوال القديمة (AES عادي - للترخيص فقط)
        // ═══════════════════════════════════════════════════════
        
        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GenerateRandomData(byte[] buffer, int size);

        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateAESContext(byte[] key);

        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ProcessFrame(IntPtr ctx, byte[] data, int len, byte[] iv, long offset);

        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyAESContext(IntPtr ctx);

        // ═══════════════════════════════════════════════════════
        // القسم 2: الدوال الجديدة (WhiteBox - للفيديو)
        // ═══════════════════════════════════════════════════════
        
        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WB_Encrypt_Video(byte[] data, int dataLength, byte[] output);

        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void WB_Decrypt_Video(byte[] data, int dataLength, byte[] output);

        // ═════════════════════════════════════════════════════
        // المتغيرات
        // ═══════════════════════════════════════════════════════
        
        private IntPtr _aesContext = IntPtr.Zero;
        private byte[] _currentIv;

        // ═══════════════════════════════════════════════════════
        // دوال مساعدة عامة
        // ═══════════════════════════════════════════════════════
        
        public static byte[] GenerateRandomBytes(int size)
        {
            byte[] data = new byte[size];
            GenerateRandomData(data, size);
            return data;
        }

        public byte[] GenerateRandomData(int size)
        {
            return GenerateRandomBytes(size);
        }

        // ═══════════════════════════════════════════════════════
        // دوال التشفير القديمة (للترخيص - نبقيها)
        // ═══════════════════════════════════════════════════════
        
        public void Initialize(byte[] key, byte[] iv)
        {
            if (_aesContext != IntPtr.Zero) Dispose();

            _currentIv = iv;
            _aesContext = CreateAESContext(key);

            if (_aesContext == IntPtr.Zero)
                throw new Exception("Failed to initialize AES engine.");
        }

        public void ProcessChunk(byte[] buffer, int length, long fileOffset)
        {
            if (_aesContext == IntPtr.Zero) 
                throw new InvalidOperationException("Engine not initialized.");

            bool success = ProcessFrame(_aesContext, buffer, length, _currentIv, fileOffset);

            if (!success) 
                throw new Exception("Encryption/Decryption failed inside C++.");
        }

        // ═══════════════════════════════════════════════════════
        // دوال WhiteBox الجديدة (للفيديو)
        // ═══════════════════════════════════════════════════════
        
        /// <summary>
        /// تشفير بيانات باستخدام WhiteBox (بدون مفتاح!)
        /// </summary>
        public static byte[] EncryptWithWhiteBox(byte[] data)
        {
            byte[] output = new byte[data.Length];
            WB_Encrypt_Video(data, data.Length, output);
            return output;
        }

        /// <summary>
        /// فك تشفير بيانات باستخدام WhiteBox (بدون مفتاح!)
        /// </summary>
        public static byte[] DecryptWithWhiteBox(byte[] data)
        {
            byte[] output = new byte[data.Length];
            WB_Decrypt_Video(data, data.Length, output);
            return output;
        }

        // ═══════════════════════════════════════════════════════
        // التنظيف
        // ═══════════════════════════════════════════════════════
        
        public void Dispose()
        {
            if (_aesContext != IntPtr.Zero)
            {
                DestroyAESContext(_aesContext);
                _aesContext = IntPtr.Zero;
            }
        }
    }
}
