namespace SlideDesignUnlocker;

internal class SlideModel : ObservableObject
{
    private string? title;
    private bool hasElementsWithLocks;

    public SlideModel()
    {
        this.Shapes.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                HasElementsWithLocks = false;
            }
            else if(e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?.OfType<ShapeModel>().Any(s => s.HasLocks) ?? false)
                {
                    HasElementsWithLocks = true;
                }
            }
        };
    }

    public string? Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    public bool HasElementsWithLocks
    {
        get => hasElementsWithLocks;
        set => SetProperty(ref hasElementsWithLocks, value);
    }

    public ObservableCollection<ShapeModel> Shapes { get; } = new();
}
