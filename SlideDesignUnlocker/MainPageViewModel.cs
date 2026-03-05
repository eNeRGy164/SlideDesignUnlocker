namespace SlideDesignUnlocker;

internal partial class MainPageViewModel : ObservableObject, IRecipient<SlideChangeStatusChanged>
{
    public MainPageViewModel()
    {
        WeakReferenceMessenger.Default.Register<SlideChangeStatusChanged>(this);
    }

    [ObservableProperty]
    public partial string? FilePath { get; set; }

    /// <summary>
    /// Stores the hash of the file when it was opened, used to detect external modifications.
    /// </summary>
    internal string? FileHash { get; set; }

    [ObservableProperty]
    public partial string? Error { get; set; }

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; }

    [ObservableProperty]
    public partial bool Saving { get; set; }

    [ObservableProperty]
    public partial SlideModel? SelectedSlide { get; set; }

    [ObservableProperty]
    public partial ShapeModel? SelectedShape { get; set; }

    [ObservableProperty]
    public partial bool SlidesChanged { get; set; }

    internal ObservableCollection<SlideModel> Slides { get; } = [];

    public void Receive(SlideChangeStatusChanged _)
    {
        this.SlidesChanged = this.Slides.Any(s => s.HasElementsWithChanges);
    }
}
