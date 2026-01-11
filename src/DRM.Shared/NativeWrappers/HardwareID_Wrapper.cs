using System;
using System.Runtime.InteropServices;
using DRM.Shared.Models;

namespace DRM.Shared.NativeWrappers
{
    public static class HardwareID_Wrapper
    {
        private const string DllName = "DRM.Security.Native.dll";

        // استيراد دالة C++
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "GetDeviceFingerprint")]
        private static extern IntPtr GetDeviceFingerprintNative();

        // ✅ الدالة العامة الأصلية
        public static CoreResponse GetDeviceFingerprint()
        {
            return GetRealHardwareFingerprint();
        }

        // ✅ الدالة العامة
        public static CoreResponse GetRealHardwareFingerprint()
        {
            try
            {
                // استدعاء C++
                IntPtr ptr = GetDeviceFingerprintNative();
                string resultString = Marshal.PtrToStringAnsi(ptr);

                // التحقق من الأخطاء والـ VM
                if (string.IsNullOrEmpty(resultString) || resultString.Contains("ERROR"))
                {
                    return new CoreResponse { Success = false, Message = "فشل: " + resultString };
                }

                if (resultString.Contains("BLOCK") || resultString.Contains("VM_DETECTED"))
                {
                    return new CoreResponse { Success = false, Message = "تم كشف بيئة وهمية (VM Detected)!" };
                }

                // نجاح
                return new CoreResponse { Success = true, Data = resultString };
            }
            catch (DllNotFoundException)
            {
                return new CoreResponse { Success = false, Message = "ملف DRM.Security.Native.dll مفقود!" };
            }
            catch (Exception ex)
            {
                return new CoreResponse { Success = false, Message = "خطأ غير متوقع: " + ex.Message };
            }
            // ملاحظة: لا نحتاج FreeMem لأن الـ DLL يستخدم buffer ثابت
        }
    }
}
