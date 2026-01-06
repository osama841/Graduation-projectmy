// HardwareID_Wrapper.cs
using DRM.Shared.Models; // لتضمين CoreResponse
using System.Runtime.InteropServices;

namespace DRM.Shared.NativeWrappers
{
    public static class HardwareID_Wrapper
    {
        private const string DllName = "DRM.Security.Native.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetDeviceFingerprintJson();

        public static CoreResponse GetDeviceFingerprint()
        {
            return NativeCallHelper.Execute(() => GetDeviceFingerprintJson());
        }
    }
}