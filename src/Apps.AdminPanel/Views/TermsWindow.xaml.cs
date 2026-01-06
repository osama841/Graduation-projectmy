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
    /// Interaction logic for TermsWindow.xaml
    /// </summary>
    public partial class TermsWindow : Window
    {
        public TermsWindow()
        {
            InitializeComponent();
        }

       // لتحريك النافذة
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        // زر الموافقة
        private void BtnAgree_Click(object sender, RoutedEventArgs e)
        {
            // هنا تفتح النافذة الرئيسية (Dashboard)
            WelcomeWindow loader = new WelcomeWindow();
            loader.Show();

            // إغلاق الشروط
            this.Close();
        }

        // زر الخروج
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // إغلاق التطبيق بالكامل
            Application.Current.Shutdown();
        }

    }
}
