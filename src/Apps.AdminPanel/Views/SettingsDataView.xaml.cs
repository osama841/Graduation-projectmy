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
    /// Interaction logic for SettingsDataView.xaml
    /// </summary>
    public partial class SettingsDataView : UserControl
    {
        public SettingsDataView()
        {
            InitializeComponent();
        }
        // 1. زر النسخ الاحتياطي
        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("جاري إنشاء نسخة احتياطية جديدة...", "النظام", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 2. زر الاستعادة
        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("حدد ملف النسخة الاحتياطية لاستعادتها.", "استعادة", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        // 3. زر التنظيف
        private void BtnCleanup_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تم تنظيف السجلات القديمة بنجاح.\nتم توفير 520 MB.", "تنظيف", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 4. زر التصدير
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تم تصدير ملف المستخدمين (Users.csv) إلى سطح المكتب.", "تصدير", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 5. زر الاستيراد
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("الرجاء اختيار ملف لاستيراده.", "استيراد", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    
}
}
