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
    /// Interaction logic for SettingsGeneralView.xaml
    /// </summary>
    public partial class SettingsGeneralView : UserControl
    {
        public SettingsGeneralView()
        {
            InitializeComponent();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // هنا يمكنك إضافة TechMessageBox لإظهار رسالة النجاح
            MessageBox.Show("تم حفظ الإعدادات بنجاح!");
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تمت استعادة الإعدادات الافتراضية.");
        }
    }
}
