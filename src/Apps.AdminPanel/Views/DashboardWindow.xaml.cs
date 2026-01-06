using Apps.AdminPanel.Components;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.Metadata;
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
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        DashboardHomeView dashboardHomeView = new DashboardHomeView();
        // نقوم بإنشاء نسخة من صفحة الإعدادات
        SettingsGeneralView settingsPage = new SettingsGeneralView();
        // إنشاء صفحة الأمان
        SettingsSecurityView securityPage = new SettingsSecurityView();
        // إنشاء صفحة معلومات النظام
        SettingsSystemView systemPage = new SettingsSystemView();
        public DashboardWindow()
        {
            InitializeComponent();
        
            MainContentArea.Content = dashboardHomeView;
            UpdateButtonVisuals(BtnDashboard);
     
         
        }
        // هذه الدالة هي المسؤولة عن "نقل" اللون الأخضر للزر النشط
        private void UpdateButtonVisuals(TechButton activeButton)
        {
            // ألوان التطبيق (نجلبها من Resources)
            var accentColor = (Brush)FindResource("AccentColor");
            var textSecondary = (Brush)FindResource("TextSecondary");

            // لون الخلفية الشفاف للزر النشط (أخضر خفيف)
            var activeBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1500E676"));

            // قائمة بكل أزرار القائمة الجانبية
            var allButtons = new[] { BtnDashboard, BtnEncrypted, BtnEncryption, BtnLocal,Data,Users,Settings };

            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                if (btn == activeButton)
                {
                    // حالة النشاط (Active)
                    btn.Background = activeBackground;
                    btn.Foreground = accentColor;
                }
                else
                {
                    // حالة الخمول (Inactive)
                    btn.Background = Brushes.Transparent;
                    btn.Foreground = textSecondary;
                }
            }
        }
       
  
    
        // دالة موحدة لجميع الأزرار (اربطها بحدث Click لكل زر)
        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as TechButton;
            if (clickedButton == null) return;

            // 1. تحديث الألوان
            UpdateButtonVisuals(clickedButton);

            // 2. التنقل للصفحة المطلوبة
            NavigateToPage(clickedButton.Name);
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // العودة لشاشة الدخول
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
        private void NavigateToPage(string s)
        {

            switch (s)
                {
                  case "BtnDashboard":MainContentArea.Content =dashboardHomeView; break;
                  case "BtnEncrypted": MainContentArea.Content = new EncryptedCntent();break;
                  case "BtnEncryption": MainContentArea.Content = new EncryptedCntent();break;
                  case "BtnLocal": MainContentArea.Content = new LicenseManagerView();break;
                  case "Users": MainContentArea.Content = new SettingsUsersView();break;
                case "Data": MainContentArea.Content = new SettingsDataView();break;
                
                case "Settings": MainContentArea.Content = systemPage; break;
          
                }
            }
        }
    }