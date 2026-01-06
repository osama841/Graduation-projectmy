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
using System.Windows.Threading;

namespace Apps.AdminPanel.Views
{
    /// <summary>
    /// Interaction logic for IntroWindow.xaml
    /// </summary>
    public partial class IntroWindow : Window
    {
        public IntroWindow()
        {
           InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // إنشاء مؤقت لمدة 3 ثوانٍ
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, args) =>
            {
                timer.Stop();

                // الانتقال إلى الخطوة التالية: نافذة الشروط
                TermsWindow terms = new TermsWindow();
                terms.Show();

                // إغلاق الشعار
                this.Close();
            };
            timer.Start();
        }
    }
}
