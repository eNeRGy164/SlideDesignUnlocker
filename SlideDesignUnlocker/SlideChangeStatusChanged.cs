namespace SlideDesignUnlocker;

internal class SlideChangeStatusChanged : ValueChangedMessage<SlideModel>
{
    public SlideChangeStatusChanged(SlideModel value) : base(value)
    {
    }
}
