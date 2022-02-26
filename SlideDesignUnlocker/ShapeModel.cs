namespace SlideDesignUnlocker;

[ObservableObject]
internal partial class ShapeModel
{
    private readonly Dictionary<string, bool> InitialState = new();

    public ShapeModel()
    {
        this.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(this.HasChanges)) {
                WeakReferenceMessenger.Default.Send(new ShapeChangeStatusChanged(this));

                return;
            };

            this.InitialState.TryAdd(e.PropertyName!, (bool)typeof(ShapeModel).GetProperty(e.PropertyName!)!.GetValue(s)!);
        };
    }

    public string? Name { get; set; }

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool isDesignElement;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool noResize;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool noMove;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool noRotation;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool noEditPoints;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool noChangeShapeType;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool noAdjustHandles;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool noChangeArrowheads;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(HasChanges))]
    private bool noTextEdit;

    public bool HasLocks => this.InitialState.Values.Any(v => v == true);

    public bool HasChanges => this.InitialState.Any(kv => ((bool)typeof(ShapeModel).GetProperty(kv.Key)!.GetValue(this)!) != kv.Value);
}
