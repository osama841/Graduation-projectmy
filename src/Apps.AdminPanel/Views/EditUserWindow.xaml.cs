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
using System.Windows.Shapes;

namespace Apps.AdminPanel.Views
{
    /// <summary>
    /// Interaction logic for EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {
        // نستقبل كائن يحتوي على البيانات (يمكنك استخدام الكلاس UserItem الذي أنشأناه سابقاً)
        public EditUserWindow(int id, string username, string email, string key, string status)
        {
            InitializeComponent();
       
        }
        // تحريك النافذة
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        // زر التحديث
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // هنا تضع كود الحفظ في قاعدة البيانات
            MessageBox.Show("تم تحديث بيانات المستخدم بنجاح.", "تم", MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true; // نغلق النافذة ونرجع نتيجة نجاح
            this.Close();
        }

        // زر الإلغاء
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
