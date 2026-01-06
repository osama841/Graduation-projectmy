using Apps.AdminPanel.ViewModels;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace Apps.AdminPanel.Views
{
    /// <summary>
    /// Interaction logic for CreateWorkspaceWindow.xaml
    /// </summary>
    public partial class CreateWorkspaceWindow : Window
    {
        // قائمة قابلة للمراقبة (عشان الجدول يتحدث تلقائياً)
        public ObservableCollection<FileItem> Files { get; set; }

        public CreateWorkspaceWindow()
        {
            InitializeComponent();
            // تهيئة القائمة ببيانات وهمية (مثل الصورة)
            Files = new ObservableCollection<FileItem>
            {
                new FileItem { Name = "video_raw_01.mp4", Size = "(200MB)", Icon = PackIconKind.FileVideo },
                new FileItem { Name = "صور_المشروع", Size = "(50 عنصر)", Icon = PackIconKind.Folder }
            };

            // ربط القائمة بالواجهة
            FilesList.ItemsSource = Files;
        }
        // تحريك النافذة
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        // 1. حدث الإفلات (Drop)
        private void DropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in droppedFiles)
                {
                    // تحديد الأيقونة (مجلد أم ملف)
                    bool isDir = Directory.Exists(file);
                    var icon = isDir ? PackIconKind.Folder : PackIconKind.FileDocument;

                    // حساب الحجم (بسيط)
                    string sizeInfo = isDir ? "(مجلد)" : $"({new FileInfo(file).Length / 1024} KB)";

                    Files.Add(new FileItem
                    {
                        Name = System.IO.Path.GetFileName(file),
                        Size = sizeInfo,
                        Icon = icon
                    });
                }
            }
        }
        // 2. زر حذف عنصر من القائمة
        private void BtnRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var fileItem = button.DataContext as FileItem;

            if (fileItem != null)
            {
                Files.Remove(fileItem);
            }
        }
        // 3. زر الإنشاء
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtWorkspaceName.Text))
            {
                MessageBox.Show("الرجاء إدخال اسم مساحة العمل.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Files.Count == 0)
            {
                MessageBox.Show("الرجاء إضافة ملفات أو مجلدات أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBox.Show($"تم إنشاء مساحة العمل '{TxtWorkspaceName.Text}' وإضافة {Files.Count} عنصر.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }

        // زر الإلغاء
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

    }
}
