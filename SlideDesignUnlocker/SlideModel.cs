namespace SlideDesignUnlocker;

internal partial class SlideModel : ObservableRecipient, IRecipient<ShapeChangeStatusChanged>
{
    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private bool hasElementsWithLocks;

    [ObservableProperty]
    private bool hasElementsWithChanges;

    private readonly HashSet<string> changedShapes = new();

    public ObservableCollection<ShapeModel> Shapes { get; } = new();

    public SlideModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        this.Shapes.CollectionChanged += (_, e) =>
        {
            this.HasElementsWithLocks = e.Action switch
            {
                NotifyCollectionChangedAction.Reset => this.HasElementsWithLocks = false,
                NotifyCollectionChangedAction.Add when e.NewItems?.OfType<ShapeModel>().Any(s => s.HasLocks) == true => true,
                _ => this.HasElementsWithLocks,
            };
        };
    }

    public void Receive(ShapeChangeStatusChanged message)
    {
        if (message.Value.HasChanges)
        {
            this.changedShapes.Add(message.Value.Name!);
        }
        else
        {
            this.changedShapes.Remove(message.Value.Name!);
        }

        this.HasElementsWithChanges = this.changedShapes.Count > 0;

        WeakReferenceMessenger.Default.Send(new SlideChangeStatusChanged(this));
    }
}
