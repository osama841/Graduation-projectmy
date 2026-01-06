using Apps.AdminPanel.Models;
using Apps.AdminPanel.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Apps.AdminPanel.Views
{
    public partial class LicenseManagerView : UserControl
    {
        private readonly ILicenseService _licenseService;
        private Stack<ObservableCollection<EncryptedPackage>> _navigationHistory;
        private Stack<string> _pathHistory; // لحفظ أسماء المجلدات (للعنوان)
        // القائمة المعروضة حالياً
        private ObservableCollection<EncryptedPackage> _currentItems;
        // القائمة الرئيسية المرتبطة بالواجهة
        private ObservableCollection<EncryptedPackage> _packages;
        // الفلتر للبحث
        private ICollectionView _packagesView;

        public LicenseManagerView()
        {
            InitializeComponent();

            // حقن الخدمة
            _licenseService = new LicenseService();
            _navigationHistory = new Stack<ObservableCollection<EncryptedPackage>>();
            _pathHistory = new Stack<string>();

        
            // تحميل البيانات
            Loaded += LicenseManagerView_Loaded;
        }

        private async void LicenseManagerView_Loaded(object sender, RoutedEventArgs e)
        {
            // نمنع التحميل المتكرر إذا كانت البيانات موجودة
            if (_packages != null) return;
            var rootData = await _licenseService.GetPackagesAsync();
            _currentItems = new ObservableCollection<EncryptedPackage>(rootData);
            BindList(_currentItems);
            PackagesList.ItemsSource = _currentItems;
            await LoadDataAsync();
        }
        private void BindList(ObservableCollection<EncryptedPackage> items)
        {
            PackagesList.ItemsSource = items;

            // تفعيل الفلتر للبحث
            _packagesView = CollectionViewSource.GetDefaultView(items);
            _packagesView.Filter = FilterItems;
        }

        // =========================================================
        private void PackagesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // التأكد من أن النقر تم على عنصر وليس في الفراغ
            var item = ((FrameworkElement)e.OriginalSource).DataContext as EncryptedPackage;

            // 1. شرط: يجب أن يكون العنصر موجوداً + يجب أن يكون "مجلداً"
            if (item != null && item.Type == ItemType.Package)
            {
                NavigateToFolder(item);
            }
        }

        private void NavigateToFolder(EncryptedPackage folder)
        {
            // أ. حفظ القائمة الحالية في التاريخ (عشان نرجع لها بعدين)
            _navigationHistory.Push(_currentItems);
            _pathHistory.Push(TxtCurrentPath.Text);

            // ب. تحديث القائمة الحالية لتصبح "محتويات المجلد"
            // ملاحظة: هنا نفترض أن Children معبأة، في الواقع قد تحتاج لجلبها من السيرفر
            _currentItems = folder.Children;
            PackagesList.ItemsSource = _currentItems;

            // ج. تحديث الواجهة (زر الرجوع والعنوان)
            BtnBack.Visibility = Visibility.Visible;
            TxtCurrentPath.Text = folder.Name;

            // تصفير البحث عند الدخول لمجلد جديد
            TxtSearch.Text = "";
        }

        // =========================================================
        // زر الرجوع (الخروج من المجلد)
        // =========================================================
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationHistory.Count > 0)
            {
                // أ. استرجاع القائمة السابقة
                var previousItems = _navigationHistory.Pop();
                var previousTitle = _pathHistory.Pop();

                // ب. تعيينها كقائمة حالية
                _currentItems = previousItems;
                PackagesList.ItemsSource = _currentItems;
                TxtCurrentPath.Text = previousTitle;

                // ج. إخفاء زر الرجوع إذا وصلنا للصفحة الرئيسية
                if (_navigationHistory.Count == 0)
                {
                    BtnBack.Visibility = Visibility.Collapsed;
                    TxtCurrentPath.Text = "إدارة التراخيص";
                }
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // يمكن إضافة Loading Spinner هنا
                var data = await _licenseService.GetPackagesAsync();

                _packages = new ObservableCollection<EncryptedPackage>(data);
                PackagesList.ItemsSource = _packages;

                // إعداد الفلتر (البحث)
                _packagesView = CollectionViewSource.GetDefaultView(_packages);
                _packagesView.Filter = PackageFilter;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل البيانات: {ex.Message}");
            }
        }

        // =========================================================
        // 1. منطق البحث
        // =========================================================
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // تحديث الفلتر عند كل حرف
            _packagesView?.Refresh();
        }

        private bool PackageFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(TxtSearch.Text)) return true;

            var pkg = item as EncryptedPackage;
            // البحث في اسم الحزمة
            return pkg.Name.IndexOf(TxtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
     

        private bool FilterItems(object obj)
        {
            if (string.IsNullOrWhiteSpace(TxtSearch.Text)) return true;

            var item = obj as EncryptedPackage;
            return item.Name.Contains(TxtSearch.Text);
        }
        // =========================================================
        // 2. منطق التحديث (Refresh)
        // =========================================================
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        // =========================================================
        // 3. منطق إصدار الترخيص (الزر الرئيسي)
        // =========================================================
        private async void BtnGrantLicense_Click(object sender, RoutedEventArgs e)
        {
            // التأكد من اختيار حزمة
            if (PackagesList.SelectedItem is not EncryptedPackage selectedPackage)
            {
                MessageBox.Show("الرجاء تحديد حزمة مشفرة أولاً.");
                return;
            }

            // فتح نافذة الترخيص التي صممناها سابقاً (LicenseWindow)
            // سنفترض أننا قمنا بإنشائها مسبقاً
            // يمكن تمرير اسم الحزمة للنافذة لتعرضه
            //var licenseWindow = new EditUserWindow();
         //    licenseWindow.SetPackageName(selectedPackage.Name); // إذا أردت تمرير البيانات

            // عرض النافذة كانتظار (Modal)
          //  if (licenseWindow.ShowDialog() == true)
            {
                // إذا ضغط المستخدم "حفظ" في النافذة المنبثقة
                // نفترض أن النافذة تعيد لنا اسم المستخدم الجديد
                string newUserName = "مستخدم جديد"; // (سيأتي من النافذة)

                try
                {
                    // استدعاء الخدمة لإنشاء الترخيص فعلياً
                    await _licenseService.GrantLicenseAsync(selectedPackage.Id, newUserName, DateTime.Now.AddMonths(1));

                    // تحديث الواجهة فوراً (إضافة المستخدم للقائمة في لوحة التفاصيل)
                    selectedPackage.ActiveUsers.Add(new ActiveUser
                   {
                       UserName = newUserName,
                       ExpiryDate = DateTime.Now.AddMonths(1),
                        LicenseStatus = "جديد"
                   });

                    MessageBox.Show("تم إصدار الترخيص بنجاح!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}");
                }
            }
        }
    }
}
