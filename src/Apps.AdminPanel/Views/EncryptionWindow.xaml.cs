using Apps.AdminPanel.Components;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Apps.AdminPanel.Views
{
    /// <summary>
    /// Interaction logic for EncryptionWindow.xaml
    /// </summary>
    public partial class EncryptionWindow : UserControl
    {
        public EncryptionWindow()
        {
            InitializeComponent();
        }

        // 1. عند الضغط لاختيار ملف
        private void FileArea_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                // عرض تفاصيل الملف
                TxtFileName.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
                TxtFileSize.Text = "3.2 MB"; // (محاكاة للحجم)
            }
        }

        // 2. عند سحب الملف وإفلاته
        private void FileArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    TxtFileName.Text = System.IO.Path.GetFileName(files[0]);
                    TxtFileSize.Text = "1.8 MB";
                }
            }
        }

        // 3. توليد بصمة الجهاز الحالي
        private void BtnGenerateID_Click(object sender, RoutedEventArgs e)
        {
            // محاكاة توليد ID جديد
            string newId = $"{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 4)}-{DateTime.Now.Year}";
            TxtDeviceID.Text = newId.ToUpper();
        }

        // 4. إدخال يدوي (مسح الحقل)
        private void BtnManualID_Click(object sender, RoutedEventArgs e)
        {
            TxtDeviceID.Text = "";
            TxtDeviceID.Focus();
        }

        // 5. زر بدء التشفير
        private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            // بدلاً من الرسالة الفورية، ننتقل لواجهة الحالة
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                // نمرر اسم الملف والآيدي للصفحة الجديدة (اختياري)
                var statusView = new EncryptionStatusView();
                // يمكنك تعديل EncryptionStatusView لاستقبال البيانات في الـ Constructor
                 statusView.SetDetails(TxtFileName.Text, TxtDeviceID.Text);
                dashboard.MainContentArea.Content = statusView;
            }
            // نستخدم TechMessageBox الذي صممناه سابقاً لعرض النتيجة

            TechMessageBox msg = new TechMessageBox(
                "تم التشفير بنجاح",
                $"تم تشفير الملف {TxtFileName.Text} بنجاح وربطه بالبصمة {TxtDeviceID.Text}",
                MessageType.Success,
                "فتح المجلد"
            );
            msg.ShowDialog();
        }
    }
}