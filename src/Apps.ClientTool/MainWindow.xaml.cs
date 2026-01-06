using System.Windows;
using DRM.Shared.NativeWrappers; // لاستخدام الـ Wrapper

namespace Apps.ClientTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnExtractFingerprint_Click(object sender, RoutedEventArgs e)
        {
            // 1. استدعاء الدالة (لاحظ البساطة هنا!)
            var response = HardwareID_Wrapper.GetDeviceFingerprint();

            // 2. التحقق من النتيجة
            if (response.Success)
            {
                TxtFingerprint.Text = response.Data;
                // عرض البصمة
                MessageBox.Show($"تم بنجاح!\nالبصمة: {response.Data}", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // عرض الخطأ
                MessageBox.Show($"فشل!\nالسبب: {response.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // 3. (الأهم) عرض سجلات التتبع القادمة من C++ (للتأكد أن اللوجر يعمل)
            //string logs = string.Join("\n", response.Logs);
         //   MessageBox.Show($"سجلات التتبع من النواة (C++ Logs):\n\n{logs}", "Debug Info");
        }
        private void BtnCopyFingerprint_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtFingerprint.Text))
            {
                TxtFingerprint2.Text=TxtFingerprint.Text;
                Clipboard.SetText(TxtFingerprint.Text);
                MessageBox.Show("تم نسخ البصمة إلى الحافظة.", "نسخ", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

}