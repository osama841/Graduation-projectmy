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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Apps.AdminPanel.Views
{
    /// <summary>
    /// Interaction logic for EncryptionStatusView.xaml
    /// </summary>
    public partial class EncryptionStatusView : UserControl
    {
        private DispatcherTimer _timer;
        private int _progress = 0;
        private int _secondsElapsed = 0;
        public EncryptionStatusView()
        {
            InitializeComponent();
        }
        public void SetDetails(string details, string key)
        {
            if (details == null)
            {
                MessageBox.Show("error");
            }
            else {
                TxtFileName.Text = details;
                TxtTargetID.Text = key;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. تشغيل أنيميشن الدوران
            Storyboard spin = (Storyboard)this.Resources["SpinAnimation"];
            spin.Begin();

            // 2. بدء المؤقت لمحاكاة التقدم
            StartSimulation();
        }
        private void StartSimulation()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100); // سرعة التحديث
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            _progress++;

            // تحديث الشريط والنص
            MainProgressBar.Value = _progress;
            TxtPercent.Text = $"{_progress}%";

            // تحديث الوقت كل ثانية تقريباً (كل 10 دقات)
            if (_progress % 10 == 0)
            {
                _secondsElapsed++;
                TxtElapsedTime.Text = TimeSpan.FromSeconds(_secondsElapsed).ToString(@"mm\:ss");
                // وقت متبقي تقريبي
                TxtRemainingTime.Text = TimeSpan.FromSeconds(15 - _secondsElapsed).ToString(@"mm\:ss");
            }

            // انتهاء العملية
            if (_progress >= 100)
            {
                _timer.Stop();
                FinishEncryption();
            }
        }
        private void FinishEncryption()
        {
            // إظهار رسالة النجاح
            MessageBox.Show("تم تشفير الملف بنجاح!", "اكتمل", MessageBoxButton.OK, MessageBoxImage.Information);

            // العودة للشاشة الرئيسية أو شاشة التشفير
            // (هنا نفترض أننا نعود لواجهة التشفير الفارغة)
            // يمكنك استخدام Event لتخبر النافذة الرئيسية بالعودة
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                dashboard.MainContentArea.Content = new EncryptionWindow();
            }
        }
        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {
            // نعود للوحة التحكم الرئيسية ونترك العملية تعمل في الخلفية (نظرياً)
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                // dashboard.MainContentArea.Content = new DashboardHomeView(); // مثال
                MessageBox.Show("تم إخفاء العملية في الخلفية.");
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null) _timer.Stop();

            MessageBoxResult result = MessageBox.Show("هل أنت متأكد من إلغاء عملية التشفير؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // العودة
                if (Window.GetWindow(this) is DashboardWindow dashboard)
                {
                    dashboard.MainContentArea.Content = new EncryptionWindow();
                }
            }
            else
                _timer.Start();
        }
        

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                MessageBox.Show("تم إيقاف العملية مؤقتاً.");
            }
            else
            {
                _timer.Start();
            }
        }
    }
}
        
