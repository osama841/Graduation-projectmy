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
    /// Interaction logic for SettingsSecurityView.xaml
    /// </summary>
    public partial class SettingsSecurityView : UserControl
    {
        public SettingsSecurityView()
        {
            InitializeComponent();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // محاكاة حفظ إعدادات الأمان
            MessageBox.Show("تم تحديث بروتوكولات الأمان بنجاح.", "النظام الآمن", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تم استعادة إعدادات الأمان الافتراضية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
