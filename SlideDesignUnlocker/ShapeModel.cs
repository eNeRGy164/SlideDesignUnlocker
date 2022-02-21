namespace SlideDesignUnlocker;

internal class ShapeModel : ObservableObject
{
    private string? name;
    private bool hasLocks;

    private bool noResize;
    private bool noMove;
    private bool noRotation;
    private bool noEditPoints;
    private bool noChangeShapeType;
    private bool noAdjustHandles;
    private bool noChangeArrowheads;
    private bool noSelection;
    private bool noTextEdit;

    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public bool HasLocks
    {
        get => hasLocks;
        set => SetProperty(ref hasLocks, value);
    }

    public bool NoSelection
    {
        get => noSelection;
        set
        {
            SetProperty(ref noSelection, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }

    public bool NoTextEdit
    {
        get => noTextEdit;
        set
        {
            SetProperty(ref noTextEdit, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }

    public bool NoResize
    {
        get => noResize;
        set
        {
            SetProperty(ref noResize, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }

    public bool NoMove
    {
        get => noMove;
        set
        {
            SetProperty(ref noMove, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }

    public bool NoRotation
    {
        get => noRotation;
        set
        {
            SetProperty(ref noRotation, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }

    public bool NoEditPoints
    {
        get => noEditPoints;
        set
        {
            SetProperty(ref noEditPoints, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }

    public bool NoChangeShapeType
    {
        get => noChangeShapeType;
        set
        {
            SetProperty(ref noChangeShapeType, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }

    public bool NoAdjustHandles
    {
        get => noAdjustHandles;
        set
        {
            SetProperty(ref noAdjustHandles, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }

    public bool NoChangeArrowheads
    {
        get => noChangeArrowheads;
        set
        {
            SetProperty(ref noChangeArrowheads, value);
            if (value) SetProperty(ref hasLocks, value);
        }
    }
}
