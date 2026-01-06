
using System;
using System.Runtime.InteropServices;
using DRM.Shared.Models;
using Newtonsoft.Json;

namespace DRM.Shared.NativeWrappers
{
    public static class NativeCallHelper
    {
        // استيراد دالة تحرير الذاكرة
        [DllImport("DRM.Security.Native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeMem(IntPtr ptr);

        // دالة عامة (Generic) لتنفيذ أي استدعاء للنواة
        public static CoreResponse Execute(Func<IntPtr> nativeMethod)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                // 1. استدعاء دالة C++
                ptr = nativeMethod();

                // 2. تحويل المؤشر إلى نص
                string jsonResponse = Marshal.PtrToStringAnsi(ptr);

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return new CoreResponse
                    {
                        Success = false,
                        Message = "Fatal Error: Native core returned empty response."
                    };
                }

                // 3. تحويل JSON إلى كائن C#
                var response = JsonConvert.DeserializeObject<CoreResponse>(jsonResponse);
                return response ?? new CoreResponse { Success = false, Message = "Failed to deserialize response." };
            }
            catch (Exception ex)
            {
                // التقاط أي خطأ غير متوقع في الربط
                return new CoreResponse
                {
                    Success = false,
                    Message = $"Wrapper Exception: {ex.Message}",
                    Logs = new System.Collections.Generic.List<string> { ex.ToString() }
                };
            }
            finally
            {
                // 4. تنظيف الذاكرة دائماً
                if (ptr != IntPtr.Zero) FreeMem(ptr);
            }
        }
    }
}