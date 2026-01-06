using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SecurePlayer.Pages
{
    public partial class PdfViewerPage : Page
    {
        private string? currentPdfPath;
        private double currentZoom = 1.0;

        public PdfViewerPage()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                await PdfWebView.EnsureCoreWebView2Async(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF Files|*.pdf|All Files|*.*";
            openFileDialog.Title = "Select a PDF File";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    currentPdfPath = openFileDialog.FileName;

                    // Load PDF in WebView2
                    PdfWebView.Source = new Uri(currentPdfPath);
                    PdfWebView.Visibility = Visibility.Visible;
                    NoPdfPanel.Visibility = Visibility.Collapsed;

                    // Update UI
                    FileNameText.Text = Path.GetFileName(currentPdfPath);
                    FileSizeText.Text = FormatFileSize(new FileInfo(currentPdfPath).Length);
                    TotalPagesText.Text = "N/A";
                    CurrentPageText.Text = "1";

                    // Enable scroll buttons
                    PrevButton.IsEnabled = true;
                    NextButton.IsEnabled = true;

                    MessageBox.Show("PDF loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (PdfWebView.CoreWebView2 != null)
            {
                await PdfWebView.CoreWebView2.ExecuteScriptAsync("window.scrollBy(0, -window.innerHeight);");
            }
        }

        private async void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (PdfWebView.CoreWebView2 != null)
            {
                await PdfWebView.CoreWebView2.ExecuteScriptAsync("window.scrollBy(0, window.innerHeight);");
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (currentZoom < 3.0)
            {
                currentZoom += 0.1;
                PdfWebView.ZoomFactor = currentZoom;
                ZoomText.Text = $"{(int)(currentZoom * 100)}%";
            }
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (currentZoom > 0.5)
            {
                currentZoom -= 0.1;
                PdfWebView.ZoomFactor = currentZoom;
                ZoomText.Text = $"{(int)(currentZoom * 100)}%";
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
