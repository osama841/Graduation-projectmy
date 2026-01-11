using System;
using System.Runtime.InteropServices;
using System.Windows;
using DRM.Shared.NativeWrappers;

namespace SecurePlayer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // ✅ استدعاء الدالة للتحقق من بصمة الجهاز
                var authResult = HardwareID_Wrapper.GetRealHardwareFingerprint();

                // التحقق من النتيجة
                if (!authResult.Success)
                {
                    MessageBox.Show(
                        "فشل التحقق:\n" + authResult.Message,
                        "SecureGate Protection",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    Shutdown();
                    return;
                }

                // نجاح - عرض البصمة
                MessageBox.Show(
                    "✅ تم التحقق: جهاز حقيقي!\nالبصمة: " + authResult.Data,
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // إكمال التشغيل
                base.OnStartup(e);
            }
            catch (DllNotFoundException ex)
            {
                MessageBox.Show(
                    "ملف DRM.Security.Native.dll غير موجود!\n\n" + ex.Message,
                    "خطأ DLL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }
            catch (SEHException ex)
            {
                MessageBox.Show(
                    "خطأ في كود C++ (SEH Exception):\n" + ex.Message + "\n\nErrorCode: " + ex.ErrorCode,
                    "خطأ Native Code",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }
            catch (AccessViolationException ex)
            {
                MessageBox.Show(
                    "خطأ في الوصول للذاكرة:\n" + ex.Message,
                    "Access Violation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطأ غير متوقع:\n" + ex.GetType().Name + "\n" + ex.Message + "\n\nStackTrace:\n" + ex.StackTrace,
                    "خطأ عام",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }
        }
    }
}
