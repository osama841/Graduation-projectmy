using DRM.Shared.Models;
using DRM.Shared.NativeWrappers;
using System.Threading.Tasks;

namespace SecurePlayer.Services
{
    public class DeviceFingerprintService
    {
        public async Task<string> GetDeviceFingerprintAsync()
        {
             // Run on background thread as it might be heavy (hardware access)
             return await Task.Run(() => 
             {
                 CoreResponse response = HardwareID_Wrapper.GetDeviceFingerprint();
                 
                 if (response.Success && !string.IsNullOrEmpty(response.Data))
                 {
                     return response.Data;
                 }
                 else
                 {
                     // Return error or throw exception? For now, return empty or error message
                     return string.Empty; 
                 }
             });
        }
    }
}
