using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SlideDesignUnlocker;
using System.Collections.ObjectModel;

namespace SlideDesignUnlocker.Mac;

internal partial class MainPageViewModel : ObservableObject, IRecipient<SlideChangeStatusChanged>
{
    public MainPageViewModel()
    {
        WeakReferenceMessenger.Default.Register<SlideChangeStatusChanged>(this);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFile))]
    [NotifyPropertyChangedFor(nameof(NoFile))]
    public partial string? FilePath { get; set; }

    /// <summary>
    /// The user-selected file path (security-scoped on Mac Catalyst).
    /// </summary>
    internal string? OriginalFilePath { get; set; }

    /// <summary>
    /// A working copy in the app cache directory, used for sandbox-safe OpenXml access.
    /// </summary>
    internal string? WorkingFilePath { get; set; }

    /// <summary>
    /// Stores the hash of the file when it was opened, used to detect external modifications.
    /// </summary>
    internal string? FileHash { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? Error { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; }

    [ObservableProperty]
    public partial bool Saving { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedSlide))]
    public partial SlideModel? SelectedSlide { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedShape))]
    public partial ShapeModel? SelectedShape { get; set; }

    [ObservableProperty]
    public partial bool SlidesChanged { get; set; }

    public ObservableCollection<SlideModel> Slides { get; } = [];

    public bool HasFile => FilePath is not null;

    public bool NoFile => FilePath is null;

    public bool HasSelectedSlide => SelectedSlide is not null;

    public bool HasSelectedShape => SelectedShape is not null;

    public bool HasError => !string.IsNullOrEmpty(Error);

    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

    partial void OnSelectedSlideChanged(SlideModel? value)
    {
        SelectedShape = null;
    }

    public void Receive(SlideChangeStatusChanged _)
    {
        SlidesChanged = Slides.Any(s => s.HasElementsWithChanges);
    }
}
