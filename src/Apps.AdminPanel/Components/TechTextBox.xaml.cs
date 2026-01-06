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
    /// Interaction logic for TechTextBox.xaml
    /// </summary>
    public partial class TechTextBox : UserControl
    {
        public event TextChangedEventHandler TextChanged;
        public TechTextBox()
        {
            InitializeComponent();
        }
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
     

        private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // تمرير الحدث للصفحة الأب
            TextChanged?.Invoke(this, e);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TechTextBox), new PropertyMetadata(string.Empty));

        // 2. خاصية النص التوضيحي (Hint)
        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(TechTextBox), new PropertyMetadata(string.Empty));

        // 3. خاصية الأيقونة (Icon)
        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        // القيمة الافتراضية خليناها أيقونة "Pencil" مثلاً
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(PackIconKind), typeof(TechTextBox), new PropertyMetadata(PackIconKind.Pencil));
    }
}
