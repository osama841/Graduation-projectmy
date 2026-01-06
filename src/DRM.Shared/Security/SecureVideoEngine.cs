using System;
using System.IO;
using System.Runtime.InteropServices;
namespace DRM.Shared.Security
{
    public class SecureVideoEngine : IDisposable
    {
        // --- ربط دوال الـ DLL ---
        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GenerateRandomData(byte[] buffer, int size);

        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateAESContext(byte[] key);

        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ProcessFrame(IntPtr ctx, byte[] data, int len, byte[] iv, long offset);

        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyAESContext(IntPtr ctx);

        // --- متغيرات الكلاس ---
        private IntPtr _aesContext = IntPtr.Zero; // هذا هو "الكارت" الذي أعطانا إياه C++
        private byte[] _currentIv;

        // --- دوال مساعدة عامة ---

        // دالة لتوليد مفتاح أو IV عشوائي آمن
        public static byte[] GenerateRandomBytes(int size)
        {
            byte[] data = new byte[size];
            GenerateRandomData(data, size);
            return data;
        }

        // Instance wrapper for compatibility
        public byte[] GenerateRandomData(int size)
        {
            return GenerateRandomBytes(size);
        }

        // --- دورة حياة تشغيل الفيديو ---

        // 1. التهيئة (عند فتح الملف)
        public void Initialize(byte[] key, byte[] iv)
        {
            if (_aesContext != IntPtr.Zero) Dispose(); // تنظيف القديم إن وجد

            _currentIv = iv;
            // نطلب من C++ تجهيز التشفير ونحتفظ بالكارت
            _aesContext = CreateAESContext(key);

            if (_aesContext == IntPtr.Zero)
                throw new Exception("Failed to initialize AES engine.");
        }

        // 2. المعالجة (تستدعى آلاف المرات)
        public void ProcessChunk(byte[] buffer, int length, long fileOffset)
        {
            if (_aesContext == IntPtr.Zero) throw new InvalidOperationException("Engine not initialized.");

            // نمرر الكارت (_aesContext) ليعرف C++ أي مفتاح يستخدم
            bool success = ProcessFrame(_aesContext, buffer, length, _currentIv, fileOffset);

            if (!success) throw new Exception("Encryption/Decryption failed inside C++.");
        }

        // 3. التنظيف (عند إغلاق الصفحة أو الفيديو)
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