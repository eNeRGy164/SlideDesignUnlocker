namespace SlideDesignUnlocker;

[ObservableObject]
internal partial class MainPageViewModel : IRecipient<SlideChangeStatusChanged>
{
    public MainPageViewModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    [ObservableProperty]
    private string? filePath;

    [ObservableProperty]
    private string error = string.Empty;

    [ObservableProperty]
    private bool loading;

    [ObservableProperty]
    private SlideModel? selectedSlide;

    [ObservableProperty]
    private ShapeModel? selectedShape;

    [ObservableProperty]
    private bool slidesChanged;

    internal ObservableCollection<SlideModel> Slides { get; } = new();

    public void Receive(SlideChangeStatusChanged _)
    {
        this.SlidesChanged = this.Slides.Any(s => s.HasElementsWithChanges);
    }
}
