namespace SlideDesignUnlocker;

internal class ShapeChangeStatusChanged : ValueChangedMessage<ShapeModel>
{
    public ShapeChangeStatusChanged(ShapeModel value) : base(value)
    {
    }
}
