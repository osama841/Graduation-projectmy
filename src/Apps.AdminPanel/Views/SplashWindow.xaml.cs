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
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            // الاشتراك في حدث انتهاء التحميل
            MyLoader.LoadingCompleted += MyLoader_LoadingCompleted;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // بدء العداد بمجرد ظهور النافذة
            MyLoader.StartLoading();
        }

        private void MyLoader_LoadingCompleted(object sender, EventArgs e)
        {
            // عند الانتهاء: افتح نافذة الدخول وأغلق هذه النافذة
            WelcomeWindow welcome = new WelcomeWindow();
            welcome.Show();
            this.Close();
        }
    }
}
