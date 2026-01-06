using Apps.AdminPanel.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Apps.AdminPanel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // هذه أهم خطوة: بناء قاعدة البيانات عند التشغيل
            DatabaseHelper.InitializeDatabase();

            base.OnStartup(e);
        }
    }

}
