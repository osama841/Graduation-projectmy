using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SecurePlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Navigate to Welcome Page on startup
            MainFrame.Navigate(new Pages.WelcomePage());
        }

        // ====== Title Bar Functions ======

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double click to maximize/restore
                MaximizeButton_Click(sender, e);
            }
            else
            {
                // Single click to drag
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ====== Navigation Functions ======

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.DashboardPage());
            UpdateActiveButton(BtnDashboard);
        }

        private void BtnVideoPlayer_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.VideoPlayerPage());
            UpdateActiveButton(BtnVideoPlayer);
        }

        private void BtnPdfViewer_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.PdfViewerPage());
            UpdateActiveButton(BtnPdfViewer);
        }


        private async void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Call Wrapper directly to inspect full response
                var response = await System.Threading.Tasks.Task.Run(() => DRM.Shared.NativeWrappers.HardwareID_Wrapper.GetDeviceFingerprint());

                if (response.Success)
                {
                    MessageBox.Show($"Success!\nFingerprint: {response.Data}", "Security Check", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string logs = response.Logs != null ? string.Join("\n", response.Logs) : "No logs available.";
                    MessageBox.Show($"Failed!\nMessage: {response.Message}\n\nLogs:\n{logs}", "Security Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Critical Exception:\n{ex.Message}\n{ex.StackTrace}", "Security Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateActiveButton(BtnSettings);
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Help - Coming Soon!");
            UpdateActiveButton(BtnHelp);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                                         "Logout",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        // ====== Helper Functions ======

        private void UpdateActiveButton(Button activeButton)
        {
            // Reset all buttons to default style
            var inactiveColor = new SolidColorBrush(Color.FromRgb(139, 139, 154));
            
            BtnDashboard.Background = Brushes.Transparent;
            BtnDashboard.Foreground = inactiveColor;

            BtnVideoPlayer.Background = Brushes.Transparent;
            BtnVideoPlayer.Foreground = inactiveColor;

            BtnPdfViewer.Background = Brushes.Transparent;
            BtnPdfViewer.Foreground = inactiveColor;

            BtnSettings.Background = Brushes.Transparent;
            BtnSettings.Foreground = inactiveColor;

            BtnHelp.Background = Brushes.Transparent;
            BtnHelp.Foreground = inactiveColor;

            BtnLogout.Background = Brushes.Transparent;
            BtnLogout.Foreground = inactiveColor;

            // Highlight the active button with gradient
            if (activeButton != null)
            {
                var gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new System.Windows.Point(0, 0);
                gradientBrush.EndPoint = new System.Windows.Point(1, 1);
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(99, 102, 241), 0)); // #6366F1
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(139, 92, 246), 1)); // #8B5CF6
                activeButton.Background = gradientBrush;
                activeButton.Foreground = Brushes.White;
            }
        }
    }
}
