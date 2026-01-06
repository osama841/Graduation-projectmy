using MaterialDesignThemes.Wpf;
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

namespace Apps.AdminPanel.Components
{
    /// <summary>
    /// Interaction logic for TechPasswordBox.xaml
    /// </summary>
    public partial class TechPasswordBox : UserControl
    {
        public TechPasswordBox()
        {
            InitializeComponent();
        }
      //  Hint
        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(TechPasswordBox), new PropertyMetadata(string.Empty));

        // 2. خاصية الأيقونة
        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(PackIconKind), typeof(TechPasswordBox), new PropertyMetadata(PackIconKind.Lock));

        // 3. خاصية كلمة المرور (للحصول على القيمة)
        // هذه الخاصية ليست DependencyProperty عادية لأننا لا نستطيع ربطها بسهولة
        // لكننا نكشفها لكي تستطيع قراءتها من النافذة الرئيسية
        public string Password
        {
            get { return InnerPasswordBox.Password; }
            set { InnerPasswordBox.Password = value; }
        }

        // حدث لتمرير التغييرات (اختياري، يفيدك لو أردت التحقق أثناء الكتابة)
        private void InnerPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // هنا يمكنك إضافة منطق إذا أردت
        }
    }
}
