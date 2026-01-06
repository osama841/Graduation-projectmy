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
    /// Interaction logic for SettingsSystemView.xaml
    /// </summary>
    public partial class SettingsSystemView : UserControl
    {
        public SettingsSystemView()
        {
            InitializeComponent();
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // إظهار رسالة تأكيد باستخدام TechMessageBox (إذا كان متوفراً) أو رسالة عادية
            MessageBox.Show("هل أنت متأكد من استعادة إعدادات النظام للمصنع؟\nسيتم فقدان السجلات.", "تحذير أمني", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }
    }
}
