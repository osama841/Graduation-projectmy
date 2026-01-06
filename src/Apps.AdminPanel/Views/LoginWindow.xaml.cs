using Apps.AdminPanel.Components;
using Apps.AdminPanel.DBconection;
using Apps.AdminPanel.Views; // لاستيراد CreateAccountWindow لفتحها من هنا
using BCrypt.Net;
using System.Windows;
using System.Windows.Controls; // تأكد من وجود هذا الاستيراد
using System.Windows.Input;
namespace Apps.AdminPanel.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
           
        }
        // 1. حدث تحريك النافذة (لأننا لغينا الشريط العلوي WindowStyle="None")
        // اضغط بزر الماوس الأيسر في أي مكان واسحب النافذة
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        // 2. حدث زر الدخول
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // أ) جلب البيانات من مكوناتنا المخصصة
            // لاحظ السهولة: نستخدم الخصائص التي برمجناها (Text و Password)
            string username = TxtUsername.Text;
            string password = TxtPassword.Password;

            // ب) التحقق من الحقول الفارغة
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("من فضلك أدخل اسم المستخدم وكلمة المرور", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new AppDbContext())
                {
                    // 1. البحث عن المستخدم في قاعدة البيانات
                    var user = context.Users.SingleOrDefault(u => u.Username == username);

                    if (user != null)
                    {
                        // 2. التحقق من كلمة المرور
                        // user.PasswordHash هي الشفرة المخزنة في قاعدة البيانات
                        // password هي الكلمة التي كتبها المستخدم الآن
                        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                        if (isPasswordValid)
                        {
                            if (!user.IsActive)
                            {
                                MessageBox.Show("هذا الحساب معطل، يرجى مراجعة المسؤول.", "حساب موقوف", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // 3. نجاح الدخول!
                            // هنا يجب فتح النافذة الرئيسية (MainWindow)
                            // سنفترض وجود نافذة رئيسية، إذا لم تكن موجودة أنشئ واحدة فارغة للتجربة
                            DashboardWindow mainWindow = new DashboardWindow();
                            mainWindow.Show();

                            this.Close(); // إغلاق نافذة الدخول
                        }
                        else
                        {
                            MessageBox.Show("كلمة المرور غير صحيحة", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("اسم المستخدم غير موجود", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في الاتصال بقاعدة البيانات:\n{ex.Message}", "خطأ نظام", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        // 3. زر الإغلاق (اختياري: إذا أردت إضافة زر X صغير لإغلاق البرنامج)
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }}
}
