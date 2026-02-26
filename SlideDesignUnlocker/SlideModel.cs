namespace SlideDesignUnlocker;

internal partial class SlideModel : ObservableRecipient, IRecipient<ShapeChangeStatusChanged>
{
    [ObservableProperty]
    public partial string? Title { get; set; }

    /// <summary>
    /// The relationship ID used to find this slide in the presentation.
    /// </summary>
    public string? SlideRelationshipId { get; set; }

    [ObservableProperty]
    public partial bool HasElementsWithLocks { get; set; }

    [ObservableProperty]
    public partial bool HasElementsWithChanges { get; set; }

    private readonly HashSet<string> changedShapes = [];

    public ObservableCollection<ShapeModel> Shapes { get; } = [];

    public SlideModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        this.Shapes.CollectionChanged += (_, args) =>
        {
            this.HasElementsWithLocks = args.Action switch
            {
                NotifyCollectionChangedAction.Reset => this.HasElementsWithLocks = false,
                NotifyCollectionChangedAction.Add when args.NewItems?.OfType<ShapeModel>().Any(s => s.HasLocks) == true => true,
                _ => this.HasElementsWithLocks,
            };
        };
    }

    public void Receive(ShapeChangeStatusChanged message)
    {
        // Only process changes for shapes that belong to this slide
        if (!this.Shapes.Contains(message.Value))
        {
            return;
        }

        if (message.Value.HasChanges)
        {
            this.changedShapes.Add(message.Value.Name!);
        }
        else
        {
            this.changedShapes.Remove(message.Value.Name!);
        }

        this.HasElementsWithChanges = this.changedShapes.Count is not 0;

        // Recalculate HasElementsWithLocks in case locks were added or removed
        this.HasElementsWithLocks = this.Shapes.Any(s => s.HasLocks);

        WeakReferenceMessenger.Default.Send(new SlideChangeStatusChanged(this));
    }
}
