using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SlideDesignUnlocker;

public class SlideChangeStatusChanged(SlideModel value)
    : ValueChangedMessage<SlideModel>(value)
{
}
