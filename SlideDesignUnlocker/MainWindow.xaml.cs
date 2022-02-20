using Microsoft.UI.Xaml;

namespace SlideDesignUnlocker
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.RootFrame.Navigate(typeof(MainPage));
        }
    }
}
