using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SlideDesignUnlocker;

public class ShapeChangeStatusChanged(ShapeModel value)
    : ValueChangedMessage<ShapeModel>(value)
{
}
