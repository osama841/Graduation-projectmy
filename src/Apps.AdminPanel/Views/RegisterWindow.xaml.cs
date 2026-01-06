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
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }
        // 1. تحريك النافذة عند الضغط والسحب
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // 2. الانتقال إلى شاشة تسجيل الدخول
        private void HypLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        // 3. زر إنشاء الحساب
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // الحصول على القيم من المكونات
            string username = TxtRegUsername.Text;
            string email = TxtRegEmail.Text;
            string password = TxtRegPassword.Password;

            // التحقق من الحقول الفارغة
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                // يفضل استخدام TechMessageBox هنا إذا كان جاهزاً
                MessageBox.Show("الرجاء تعبئة كافة الحقول المطلوبة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- هنا يتم وضع كود حفظ البيانات في قاعدة البيانات لاحقاً ---

            // رسالة نجاح مؤقتة
            MessageBox.Show("تم إنشاء الحساب بنجاح! يمكنك الآن تسجيل الدخول.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

            // العودة لشاشة الدخول
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        // 4. زر الإغلاق العلوي
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    
}
}
