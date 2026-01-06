using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace Apps.AdminPanel.Components
{
    public partial class TechButton : UserControl
    {
        public TechButton()
        {
            InitializeComponent();
        }

        // 1. خاصية العنوان (Title)
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TechButton), new PropertyMetadata(string.Empty));

        // 2. خاصية الأيقونة (Icon)
        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(PackIconKind), typeof(TechButton), new PropertyMetadata(PackIconKind.AlertCircle));

        // 3. حدث النقر (Click Event)
        public event RoutedEventHandler Click;
        private void InnerButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        // 4. خاصية التمييز اللوني (Highlight - اختياري)
        public Brush HighlightBrush
        {
            get { return (Brush)GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }
        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register("HighlightBrush", typeof(Brush), typeof(TechButton), new PropertyMetadata(Brushes.White));
        // أضف هذا التعريف داخل كلاس TechButton لحل الخطأ
        public Brush GlowColor
        {
            get { return (Brush)GetValue(GlowColorProperty); }
            set { SetValue(GlowColorProperty, value); }
        }

        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register("GlowColor", typeof(Brush), typeof(TechButton), new PropertyMetadata(Brushes.Transparent));
    }
}