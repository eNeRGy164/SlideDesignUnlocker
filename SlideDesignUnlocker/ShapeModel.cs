namespace SlideDesignUnlocker;

internal partial class ShapeModel : ObservableObject
{
    private bool initialStateCaptured;
    private bool initialIsDesignElement;
    private bool initialNoResize;
    private bool initialNoMove;
    private bool initialNoRotation;
    private bool initialNoEditPoints;
    private bool initialNoChangeShapeType;
    private bool initialNoAdjustHandles;
    private bool initialNoChangeArrowheads;
    private bool initialNoTextEdit;
    private bool initialNoChangeAspect;
    private bool initialNoSelection;

    public ShapeModel()
    {
        this.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(this.HasChanges))
            {
                WeakReferenceMessenger.Default.Send(new ShapeChangeStatusChanged(this));
            }
        };
    }

    /// <summary>
    /// Call this after loading is complete to capture the initial state for change tracking.
    /// </summary>
    public void CaptureInitialState()
    {
        this.initialIsDesignElement = this.IsDesignElement;
        this.initialNoResize = this.NoResize;
        this.initialNoMove = this.NoMove;
        this.initialNoRotation = this.NoRotation;
        this.initialNoEditPoints = this.NoEditPoints;
        this.initialNoChangeShapeType = this.NoChangeShapeType;
        this.initialNoAdjustHandles = this.NoAdjustHandles;
        this.initialNoChangeArrowheads = this.NoChangeArrowheads;
        this.initialNoTextEdit = this.NoTextEdit;
        this.initialNoChangeAspect = this.NoChangeAspect;
        this.initialNoSelection = this.NoSelection;
        this.initialStateCaptured = true;

        // Notify that computed properties may have changed
        this.OnPropertyChanged(nameof(this.HasChanges));
        this.OnPropertyChanged(nameof(this.HasLocks));
    }

    /// <summary>
    /// The unique identifier of this shape within the slide.
    /// </summary>
    public uint ShapeId { get; set; }

    public string? Name { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool IsDesignElement { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoResize { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoMove { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoRotation { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoEditPoints { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoChangeShapeType { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoAdjustHandles { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoChangeArrowheads { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoTextEdit { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoChangeAspect { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool NoSelection { get; set; }

    public bool HasLocks =>
        this.initialIsDesignElement ||
        this.initialNoResize ||
        this.initialNoMove ||
        this.initialNoRotation ||
        this.initialNoEditPoints ||
        this.initialNoChangeShapeType ||
        this.initialNoAdjustHandles ||
        this.initialNoChangeArrowheads ||
        this.initialNoTextEdit ||
        this.initialNoChangeAspect ||
        this.initialNoSelection;

    public bool HasChanges =>
        this.initialStateCaptured && (
            this.IsDesignElement != this.initialIsDesignElement ||
            this.NoResize != this.initialNoResize ||
            this.NoMove != this.initialNoMove ||
            this.NoRotation != this.initialNoRotation ||
            this.NoEditPoints != this.initialNoEditPoints ||
            this.NoChangeShapeType != this.initialNoChangeShapeType ||
            this.NoAdjustHandles != this.initialNoAdjustHandles ||
            this.NoChangeArrowheads != this.initialNoChangeArrowheads ||
            this.NoTextEdit != this.initialNoTextEdit ||
            this.NoChangeAspect != this.initialNoChangeAspect ||
            this.NoSelection != this.initialNoSelection);
}
