namespace SlideDesignUnlocker;

[ObservableObject]
internal partial class SlideModel
{
    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private bool hasElementsWithLocks;

    public SlideModel()
    {
        this.Shapes.CollectionChanged += (_, e) =>
        {
            HasElementsWithLocks = e.Action switch
            {
                NotifyCollectionChangedAction.Reset => HasElementsWithLocks = false,
                NotifyCollectionChangedAction.Add when e.NewItems?.OfType<ShapeModel>().Any(s => s.HasLocks) == true => true,
                _ => HasElementsWithLocks,
            };
        };
    }

    public ObservableCollection<ShapeModel> Shapes { get; init; } = new();
}
