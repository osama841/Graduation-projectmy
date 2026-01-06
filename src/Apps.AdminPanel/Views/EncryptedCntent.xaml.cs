using Apps.AdminPanel.Models;   // استدعاء المودلز
using Apps.AdminPanel.Services; // استدعاء الخدمات
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Apps.AdminPanel.Views
{
    public partial class EncryptedCntent : UserControl
    {
        // تعريف الخدمة كمتغير خاص
        private readonly IEncryptionService _encryptionService;

        private readonly IFileHandlingService _fileService;

        private ObservableCollection<DrmFile> _allFiles;

        // هذا هو "الفلتر" الخاص بالجدول
        private ICollectionView _filesView;
        public EncryptedCntent()
        {
            InitializeComponent();

            // حقن التبعية (يدوياً حالياً): ننشئ نسخة من خدمة التشفير
            // هذا الأسلوب يسهل عليك تغيير طريقة التشفير مستقبلاً دون لمس هذا الملف
            _encryptionService = new EncryptionService();
            
            _fileService = new FileHandlingService();

            // تجهيز القائمة
            _allFiles = new ObservableCollection<DrmFile>();

            // ربط القائمة بالجدول
            FilesDataGrid.ItemsSource = _allFiles;

            // إعداد الفلتر (البحث)
            _filesView = CollectionViewSource.GetDefaultView(_allFiles);
            _filesView.Filter = FilterFiles; // ربط دالة الفلترة
           
        }
        private async void BtnProcessFiles_Click(object sender, RoutedEventArgs e)
        {
            // 1. التحقق
            if (FilesDataGrid.SelectedItems.Count == 0) return;

            var selectedFiles = FilesDataGrid.SelectedItems.Cast<DrmFile>().ToList();

            // 2. تجهيز الواجهة
            SwitchToProcessingMode(true);

            // 3. تجهيز المراقب
            var progressHandler = new Progress<int>(percent =>
            {
                MyProgressBar.Value = percent;
               // MyPercentText.Text = $"{percent}%";
            });

            try
            {
                // 4. استدعاء الخدمة (لاحظ النظافة هنا!)
                // الواجهة لا تعرف كيف يتم التشفير، هي فقط ترسل الطلب
                if (selectedFiles.Count == 1)
                {
                    await _encryptionService.EncryptSingleFileAsync(selectedFiles.First(), progressHandler);
                }
                else
                {
                    await _encryptionService.EncryptBatchFilesAsync(selectedFiles, progressHandler);
                }

                MessageBox.Show("تمت العملية بنجاح!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
            finally
            {
                SwitchToProcessingMode(false);
            }
        }

        // دوال التحكم بالواجهة (UI Logic) تبقى هنا لأنها تخص الـ View
        private void SwitchToProcessingMode(bool isProcessing)
        {
            PanelProcessing.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;

            if (isProcessing)
            {
                PanelDetails.Visibility = Visibility.Collapsed;
                PanelNoSelection.Visibility = Visibility.Collapsed;
            }
            else
            {
                // إعادة تقييم ما يجب عرضه
                FilesDataGrid_SelectionChanged(null, null);
            }
        }

        private void FilesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelProcessing.Visibility == Visibility.Visible) return;

            bool hasSelection = FilesDataGrid.SelectedItems.Count > 0;
            PanelDetails.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
            PanelNoSelection.Visibility = hasSelection ? Visibility.Collapsed : Visibility.Visible;

            // تحديث بيانات البنل السفلي إذا لزم الأمر
            if (hasSelection)
            {
                TxtSelectionCount.Text = $"تم تحديد {FilesDataGrid.SelectedItems.Count} ملف";
                BottomActionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                BottomActionPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            FilesDataGrid.UnselectAll();
        }

        private void BtnCancelProcess_Click(object sender, RoutedEventArgs e)
        {
            // هنا نحتاج منطق إلغاء (CancellationToken) سنضيفه لاحقاً
            SwitchToProcessingMode(false);
        }
        // 1. منطق إضافة الملفات (Import Logic)
        // =================================================================
        private void BtnAddFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true, // السماح باختيار أكثر من ملف
                Title = "اختر الملفات المراد تشفيرها",
                Filter = "All Files (*.*)|*.*|PDF Files (*.pdf)|*.pdf|Video Files (*.mp4)|*.mp4"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    // نطلب من الخدمة تحويل المسار إلى مودل جاهز
                    var newFile = _fileService.CreateFileModel(filename);

                    // نحسب الحجم النصي للعرض
                    newFile.SizeDisplay = _fileService.GetReadableFileSize(newFile.Size);

                    // إضافة للقائمة (ستظهر في الجدول فوراً)
                    _allFiles.Add(newFile);
                }
            }
        }

        // =================================================================
        // 2. منطق البحث (Search Logic)
        // =================================================================

        // هذه الدالة تتنفذ تلقائياً عند كتابة أي حرف في مربع البحث
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // تحديث الفلتر (سيقوم باستدعاء دالة FilterFiles لكل عنصر)
            _filesView.Refresh();
        }

        // دالة الفلترة الحقيقية
        private bool FilterFiles(object item)
        {
            if (item is DrmFile file)
            {
                string searchText = TxtSearch.Text; // تأكد أن اسم الـ TextBox هو TxtSearch

                // إذا كان مربع البحث فارغاً، اظهر كل شيء
                if (string.IsNullOrWhiteSpace(searchText))
                    return true;

                // البحث في الاسم (تجاهل حالة الأحرف)
                return file.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }
    }
}
