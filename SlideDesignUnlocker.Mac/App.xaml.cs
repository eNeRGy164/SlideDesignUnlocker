namespace SlideDesignUnlocker.Mac;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new NavigationPage(new MainPage()));
        window.Title = "Slide Design Unlocker";
        window.Width = 1100;
        window.Height = 700;

        return window;
    }
}
