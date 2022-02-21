using Microsoft.UI.Xaml;

namespace SlideDesignUnlocker;

[INotifyPropertyChanged]
public sealed partial class MainWindow : Window
{
    internal string AppTitle => "PowerPoint Slide Design Unlocker" + ((this.PresentationName is null) ? "" : $" - {this.PresentationName}");

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(AppTitle))]
    internal string? presentationName;

    public MainWindow()
    {
        this.InitializeComponent();

        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(this.AppTitleBar);

        this.RootFrame.Navigate(typeof(MainPage));
    }
}
