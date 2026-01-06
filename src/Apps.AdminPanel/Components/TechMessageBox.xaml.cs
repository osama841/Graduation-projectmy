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
using System.Windows.Shapes;

namespace Apps.AdminPanel.Components
{
    /// <summary>
    /// Interaction logic for TechMessageBox.xaml
    /// </summary>
    public enum MessageType
    {
        Success, // نجاح (أخضر)
        Error,   // فشل (أحمر)
        Info,    // معلومة (أزرق)
        Warning  // تحذير (أصفر)
    }
    public partial class TechMessageBox : Window
    {
        // نتيجة الزر الذي ضغطه المستخدم
        public bool IsPrimaryButtonClicked { get; private set; } = false;

        public TechMessageBox(string title, string message, MessageType type, string primaryButtonText = "موافق")
        {
            InitializeComponent();
            // تعبئة البيانات
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            BtnPrimary.Title = primaryButtonText;

            // تغيير التصميم حسب النوع
            ApplyTheme(type);
        }
        private void ApplyTheme(MessageType type)
        {
            Color glowColor;
            SolidColorBrush mainBrush;
            PackIconKind iconKind;

            switch (type)
            {
                case MessageType.Success:
                    mainBrush = (SolidColorBrush)FindResource("SuccessBrush"); // أخضر
                    glowColor = mainBrush.Color;
                    iconKind = PackIconKind.CheckCircleOutline;
                    BtnPrimary.Icon = PackIconKind.FolderOpen; // مثال
                    break;

                case MessageType.Error:
                    mainBrush = (SolidColorBrush)FindResource("ErrorBrush"); // أحمر
                    glowColor = mainBrush.Color;
                    iconKind = PackIconKind.CloseCircleOutline;
                    BtnPrimary.Icon = PackIconKind.Refresh; // مثال لإعادة المحاولة
                    break;

                // يمكنك إضافة Info و Warning هنا بنفس الطريقة
                default:
                    mainBrush = (SolidColorBrush)FindResource("NeonBlueBrush");
                    glowColor = mainBrush.Color;
                    iconKind = PackIconKind.InformationOutline;
                    break;
            }

            // تطبيق الألوان على العناصر
            MainBorder.BorderBrush = mainBrush;
            TxtTitle.Foreground = mainBrush;
            IconMsg.Foreground = mainBrush;
            IconMsg.Kind = iconKind;
         

            // تغيير لون حدود الزر الرئيسي ليناسب الحالة
            // (اختياري: يعتمد على تصميم TechButton الخاص بك)
        }

        // الزر الرئيسي (فتح المجلد / إعادة المحاولة)
        private void BtnPrimary_Click(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonClicked = true;
            this.DialogResult = true; // يعني نجاح/موافقة
            this.Close();
        }

        // زر الإغلاق
        private void BtnSecondary_Click(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonClicked = false;
            this.DialogResult = false;
            this.Close();
        }

        // دالة زر الـ X العلوي
        private void BtnCloseHeader_Click(object sender, RoutedEventArgs e)
        {
            // نعتبر الإغلاق من الـ X كأنه "إلغاء"
            IsPrimaryButtonClicked = false;
            this.DialogResult = false;
            this.Close();
        }

        // هام جداً: دالة لتحريك النافذة (Drag) عند الضغط عليها
        // لأننا لغينا شريط العنوان الأصلي، يجب أن نسمح للمستخدم بتحريك الرسالة
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

    }
}
