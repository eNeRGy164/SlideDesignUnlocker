namespace SlideDesignUnlocker;

internal partial class ShapeModel : ObservableObject
{
    [ObservableProperty]
    private string? name;
    
    [ObservableProperty]
    private bool noResize;

    [ObservableProperty]
    private bool noMove;

    [ObservableProperty]
    private bool noRotation;

    [ObservableProperty]
    private bool noEditPoints;

    [ObservableProperty]
    private bool noChangeShapeType;

    [ObservableProperty]
    private bool noAdjustHandles;

    [ObservableProperty]
    private bool noChangeArrowheads;

    [ObservableProperty]
    private bool noSelection;

    [ObservableProperty]
    private bool noTextEdit;

    public bool HasLocks => noResize || noMove || noRotation || noEditPoints || noChangeShapeType || noAdjustHandles || noChangeArrowheads || noSelection || noTextEdit;
}
