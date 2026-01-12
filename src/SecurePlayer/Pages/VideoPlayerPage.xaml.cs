using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SecurePlayer.Pages
{
    public partial class VideoPlayerPage : Page
    {
        private bool isPlaying = false;
        private bool isDraggingSlider = false;
        private DispatcherTimer timer;

        public VideoPlayerPage()
        {
            InitializeComponent();

            // Timer to update progress bar
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Encrypted Videos|*.enc|Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv|All Files|*.*";
            openFileDialog.Title = "Select a Video File";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string selectedFile = openFileDialog.FileName;
                    string fileToPlay = selectedFile;

                    // التحقق إذا كان الملف مشفر
                    if (System.IO.Path.GetExtension(selectedFile).ToLower() == ".enc")
                    {
                        // ✅ فك التشفير باستخدام WhiteBox (بدون مفتاح!)
                        string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "secure_player_decrypted.mp4");
                        
                        await System.Threading.Tasks.Task.Run(() => 
                        {
                            DecryptWithWhiteBox(selectedFile, tempFile);
                        });

                        fileToPlay = tempFile;
                    }

                    VideoPlayer.Source = new Uri(fileToPlay);
                    FileNameText.Text = System.IO.Path.GetFileName(selectedFile) + (selectedFile.EndsWith(".enc") ? " (Decrypted)" : "");
                    NoVideoPanel.Visibility = Visibility.Collapsed;
                    VideoPlayer.Play();
                    isPlaying = true;
                    timer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading video: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ✅ دالة فك التشفير باستخدام WhiteBox
        private void DecryptWithWhiteBox(string encryptedPath, string outputPath)
        {
            byte[] encryptedData = System.IO.File.ReadAllBytes(encryptedPath);
            int chunkSize = 16;
            
            using (var fsOutput = new System.IO.FileStream(outputPath, System.IO.FileMode.Create))
            {
                for (int i = 0; i < encryptedData.Length; i += chunkSize)
                {
                    int currentChunkSize = Math.Min(chunkSize, encryptedData.Length - i);
                    
                    byte[] encryptedChunk = new byte[currentChunkSize];
                    Array.Copy(encryptedData, i, encryptedChunk, 0, currentChunkSize);
                    
                    byte[] decryptedChunk = DRM.Shared.Security.SecureVideoEngine.DecryptWithWhiteBox(encryptedChunk);
                    
                    fsOutput.Write(decryptedChunk, 0, decryptedChunk.Length);
                }
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.Source == null)
            {
                MessageBox.Show("Please open a video file first!", "No Video", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (isPlaying)
            {
                VideoPlayer.Pause();
                isPlaying = false;
                timer.Stop();
            }
            else
            {
                VideoPlayer.Play();
                isPlaying = true;
                timer.Start();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!isDraggingSlider && VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                ProgressSlider.Value = VideoPlayer.Position.TotalSeconds;
                CurrentTimeText.Text = FormatTime(VideoPlayer.Position);
                TotalTimeText.Text = FormatTime(VideoPlayer.NaturalDuration.TimeSpan);
            }
        }

        private void ProgressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void ProgressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingSlider = false;
            VideoPlayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VideoPlayer.Volume = VolumeSlider.Value / 100;
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                TotalTimeText.Text = FormatTime(VideoPlayer.NaturalDuration.TimeSpan);
            }
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (LoopCheckBox.IsChecked == true)
            {
                VideoPlayer.Position = TimeSpan.Zero;
                VideoPlayer.Play();
            }
            else
            {
                isPlaying = false;
                timer.Stop();
                VideoPlayer.Position = TimeSpan.Zero;
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }
    }
}
