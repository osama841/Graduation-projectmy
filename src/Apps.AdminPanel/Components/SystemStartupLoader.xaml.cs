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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Apps.AdminPanel.Components
{
    /// <summary>
    /// Interaction logic for SystemStartupLoader.xaml
    /// </summary>
    public partial class SystemStartupLoader : UserControl
    {
        // حدث ينطلق عندما ينتهي التحميل (عشان النافذة الرئيسية تعرف متى تغلق هذا المكون)
        public event EventHandler LoadingCompleted;

        private DispatcherTimer _timer;
        private double _currentProgress = 0;

        // خاصية للربط مع الـ ProgressBars
        public double ProgressValue
        {
            get { return (double)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }
        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(double), typeof(SystemStartupLoader), new PropertyMetadata(0.0));

        // خاصية للنص (مثلاً "45%")
        public string ProgressText
        {
            get { return (string)GetValue(ProgressTextProperty); }
            set { SetValue(ProgressTextProperty, value); }
        }
        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register("ProgressText", typeof(string), typeof(SystemStartupLoader), new PropertyMetadata("0%"));
        public SystemStartupLoader()
        {
            InitializeComponent();
            InitializeTimer();
        }
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            // نريد 5 ثواني كاملة للوصول لـ 100%
            // سنحدث الشريط كل 50 ملي ثانية لكي تكون الحركة ناعمة
            // 5000ms / 50ms = 100 خطوة
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += Timer_Tick;
        }

        // دالة لبدء التحميل (تستدعى من الخارج)
        public void StartLoading()
        {
            _currentProgress = 0;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // نزيد القيمة بمقدار 1 في كل دقة (لأننا حسبنا 100 خطوة)
            _currentProgress += 1;

            ProgressValue = _currentProgress;
            ProgressText = $"{(int)_currentProgress}%";

            // عند الوصول للنهاية
            if (_currentProgress >= 100)
            {
                _timer.Stop();
                // إطلاق حدث الانتهاء
                LoadingCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    

}
}
