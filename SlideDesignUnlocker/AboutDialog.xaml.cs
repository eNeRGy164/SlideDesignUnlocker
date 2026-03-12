using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace SlideDesignUnlocker;

public sealed partial class AboutDialog : ContentDialog
{
    public string Version { get; }

    public AboutDialog()
    {
        var version = Package.Current.Id.Version;
        this.Version = $"v{version.Major}.{version.Minor}.{version.Build}";

        this.InitializeComponent();
    }
}
