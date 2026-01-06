using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SecurePlayer.Services
{
    public interface INavigationService
    {
        void NavigateTo(Page page);
        void GoBack();
        bool CanGoBack { get; }
    }

    public class NavigationService : INavigationService
    {
        private Frame _mainFrame;

        public void Initialize(Frame frame)
        {
            _mainFrame = frame;
        }

        public void NavigateTo(Page page)
        {
            _mainFrame?.Navigate(page);
        }

        public void GoBack()
        {
            if (_mainFrame != null && _mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }

        public bool CanGoBack => _mainFrame != null && _mainFrame.CanGoBack;
    }
}
