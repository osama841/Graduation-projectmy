using Apps.AdminPanel.Views;
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
    /// Interaction logic for AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        public AddUserWindow()
        {
            InitializeComponent();
        }
        // تحريك النافذة
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        // ==========================================
        // 1. منطق توليد المفتاح (Generate Key)
        // ==========================================
        private void BtnGenerateKey_Click(object sender, RoutedEventArgs e)
        {
            // توليد كود عشوائي بصيغة XXXX-XXXX-XXXX
            string key = GenerateRandomKey();
            TxtGeneratedKey.Text = key;
        }

        private string GenerateRandomKey()
        {
            // دالة بسيطة لتوليد مفتاح يشبه السيريال
            return $"{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}-" +
                   $"{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}-" +
                   $"{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}-" +
                   $"{DateTime.Now.Year}";
        }

        // ==========================================
        // 2. زر الإنشاء (Create)
        // ==========================================
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            // التحقق من الحقول
            if (string.IsNullOrWhiteSpace(TxtNewUsername.Text) ||
               
                string.IsNullOrWhiteSpace(TxtGeneratedKey.Text)) // يجب أن يكون المفتاح مولداً
            {
                MessageBox.Show("الرجاء تعبئة جميع البيانات وتوليد مفتاح الأمان.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
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




    

