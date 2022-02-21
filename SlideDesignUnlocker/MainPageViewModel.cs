namespace SlideDesignUnlocker;

[ObservableObject]
internal partial class MainPageViewModel
{
    [ObservableProperty]
    private string? filePath;

    [ObservableProperty]
    private bool loading;

    [ObservableProperty]
    private SlideModel? selectedSlide;

    [ObservableProperty]
    private ShapeModel? selectedShape;

    internal ObservableCollection<SlideModel> Slides { get; } = new();
}
