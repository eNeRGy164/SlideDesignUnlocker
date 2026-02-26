using Microsoft.UI.Xaml;
using System.ComponentModel;

namespace SlideDesignUnlocker;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    internal string AppTitle => "PowerPoint Slide Design Unlocker" + ((this.PresentationName is null) ? "" : $" - {this.PresentationName}");

    private string? presentationName;

    internal string? PresentationName
    {
        get => this.presentationName;
        set
        {
            if (this.presentationName != value)
            {
                this.presentationName = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.PresentationName)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AppTitle)));
            }
        }
    }

    public MainWindow()
    {
        this.InitializeComponent();

        this.AppWindow.Title = this.AppTitle;
        this.AppWindow.SetIcon("Assets/App.ico");

        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(this.AppTitleBar);

        this.RootFrame.Navigate(typeof(MainPage));
    }
}
