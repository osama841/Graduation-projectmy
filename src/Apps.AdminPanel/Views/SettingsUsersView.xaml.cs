using Apps.AdminPanel.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BCrypt.Net;         // للتشفير
using System.Linq;        // لدالة البحث Any


namespace Apps.AdminPanel.Views
{
    /// <summary>
    /// Interaction logic for SettingsUsersView.xaml
    /// </summary>
    public partial class SettingsUsersView : UserControl
    {
        public SettingsUsersView()
        {
            InitializeComponent();
            LoadDummyData();
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // هنا تضع كود إعادة تعيين الحقول أو تحديث البيانات
            // مثال:
            // TxtSearch.Text = "";
            // LoadData(); 
            MessageBox.Show("تمت استعادة الإعدادات الافتراضية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadDummyData()
        {
            var users = new List<UserItem>
            {
                new UserItem { Id = 1, Username = "user_admin", Email = "admin@system.com", DeviceKey = "A1B2-C3D4-E5F6", Status = "نشط", StatusColor = "#00E676" }, // أخضر
                new UserItem { Id = 2, Username = "john_doe", Email = "john@example.com", DeviceKey = "9876-5432-10AB", Status = "نشط", StatusColor = "#00E676" }, // أخضر
                new UserItem { Id = 3, Username = "jane_smith", Email = "jane@test.co", DeviceKey = "XYZ1-2345-6789", Status = "غير نشط", StatusColor = "#FF3D71" }, // أحمر
                new UserItem { Id = 4, Username = "guest_01", Email = "guest@temp.org", DeviceKey = "GUEST-0000-1111", Status = "نشط", StatusColor = "#00E676" },
                new UserItem { Id = 5, Username = "dev_team", Email = "dev@code.net", DeviceKey = "DEV-KEY-9999-88", Status = "معلق", StatusColor = "#FFB74D" } // برتقالي
            };

            UsersGrid.ItemsSource = users;
        }
        private void BtnEditUserInGrid_Click(object sender, RoutedEventArgs e)
        {
            // 1. الحصول على السطر الذي تم ضغطه
            var button = sender as Button;
            var userItem = button.DataContext as UserItem; // تأكد أن UserItem هو الكلاس الذي تستخدمه

            if (userItem != null)
            {
                // 2. فتح نافذة التعديل وإرسال البيانات لها
                // ShowDialog() مهمة جداً لأنها تجعل النافذة تظهر فوق كل شيء وتمنع الضغط خلفها
                EditUserWindow editWindow = new EditUserWindow(
                    userItem.Id,
                    userItem.Username,
                    userItem.Email,
                    userItem.DeviceKey,
                    userItem.Status
                );

                // 3. انتظار النتيجة (هل ضغط تحديث أم إلغاء؟)
                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    // قم بتحديث الجدول هنا (Reload Data)
                    // LoadDummyData();
                }
            }
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // 2. فتح نافذة التعديل وإرسال البيانات لها
            // ShowDialog() مهمة جداً لأنها تجعل النافذة تظهر فوق كل شيء وتمنع الضغط خلفها
            AddUserWindow editWindow = new AddUserWindow();

            // 3. انتظار النتيجة (هل ضغط تحديث أم إلغاء؟)
            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                // قم بتحديث الجدول هنا (Reload Data)
                // LoadDummyData();
            }
        }
        //private void BtnAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    // 1. التحقق من المدخلات (Validation)
        //    if (string.IsNullOrWhiteSpace(TxtFullName.Text) ||
        //        string.IsNullOrWhiteSpace(TxtUsername.Text) ||
        //        string.IsNullOrWhiteSpace(TxtPassword.Password))
        //    {
        //        MessageBox.Show("يرجى تعبئة الحقول الإجبارية (الاسم، المستخدم، كلمة المرور)", "خطأ إدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    try
        //    {
        //        using (var context = new Apps.AdminPanel.DBconection.AppDbContext())
        //        {
        //            // 2. التحقق من عدم تكرار اسم المستخدم
        //            if (context.Users.Any(u => u.Username == TxtUsername.Text.Trim()))
        //            {
        //                MessageBox.Show("اسم المستخدم هذا محجوز مسبقاً!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
        //                return;
        //            }

        //            // 3. تجهيز البيانات (Data Preparation)

        //            // أ) تشفير كلمة المرور
        //            string passwordHash = BCrypt.Net.BCrypt.HashPassword(TxtPassword.Password);

        //            // ب) تحديد الصلاحية (Role)
        //            string selectedRole = "User"; // القيمة الافتراضية
        //            if (CmbRole.SelectedItem is ComboBoxItem item)
        //            {
        //                selectedRole = item.Content.ToString();
        //            }

        //            // ج) توليد مفتاح الأمان الفريد (Security Key)
        //            // يولد كود مثل: A1B2C3D4E5F6
        //            string uniqueKey = BitConverter.ToString(Guid.NewGuid().ToByteArray())
        //                                .Replace("-", "").Substring(0, 16);

        //            // 4. إنشاء الكائن (Create Object)
        //            var newUser = new Apps.AdminPanel.ViewModels.User
        //            {
        //                FullName = TxtFullName.Text.Trim(),
        //                Username = TxtUsername.Text.Trim(),
        //                Email = TxtEmail.Text.Trim(),
        //                PasswordHash = passwordHash, // حفظ المشفر فقط
        //                Role = selectedRole,
        //                SecurityKey = uniqueKey,
        //                IsActive = true,
        //                CreatedAt = DateTime.Now
        //            };

        //            // 5. الحفظ في قاعدة البيانات (Save)
        //            context.Users.Add(newUser);
        //            context.SaveChanges();

        //            // 6. رسالة النجاح والإغلاق
        //            MessageBox.Show("تمت إضافة المستخدم وتوليد مفتاح الأمان بنجاح.", "تمت العملية", MessageBoxButton.OK, MessageBoxImage.Information);
        //            this.DialogResult = true; // لإشعار النافذة الأم بأن الإضافة تمت
        //            this.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"حدث خطأ أثناء الاتصال بقاعدة البيانات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تم حفظ تعديلات المستخدمين بنجاح.");
        }
    }

 
}
