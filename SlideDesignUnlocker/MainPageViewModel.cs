using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SlideDesignUnlocker
{
    internal class MainPageViewModel : ObservableObject
    {
        private string? filePath;
        private bool loading;
        private SlideModel? selectedSlide;
        private ShapeModel? selectedShape;

        internal ObservableCollection<SlideModel> Slides { get; } = new();

        internal bool Loading
        {
            get => loading;
            set => SetProperty(ref loading, value);
        }

        internal string? FilePath
        {
            get => filePath;
            set => SetProperty(ref filePath, value);
        }

        internal SlideModel? SelectedSlide
        {
            get => selectedSlide;
            set => SetProperty(ref selectedSlide, value);
        }

        internal ShapeModel? SelectedShape
        {
            get => selectedShape;
            set => SetProperty(ref selectedShape, value);
        }
    }
}
