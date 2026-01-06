using System.Windows;

namespace Apps.AdminPanel.Views
{
    public partial class RenameWindow : Window
    {
        public string NewName { get; private set; }

        public RenameWindow(string currentName)
        {
            InitializeComponent();
            TxtNewName.Text = currentName; // وضع الاسم الحالي
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewName.Text))
            {
                MessageBox.Show("الاسم لا يمكن أن يكون فارغاً");
                return;
            }
            NewName = TxtNewName.Text;
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}