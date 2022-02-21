using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Pickers;
using WinRT.Interop;
using D = DocumentFormat.OpenXml.Drawing;

namespace SlideDesignUnlocker;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        
        this.ViewModel = new MainPageViewModel();
        this.ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    internal MainPageViewModel ViewModel { get; }

    internal async void SelectFile(object _, RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker();
        filePicker.FileTypeFilter.Add(".pptx");

        InitializeWithWindow.Initialize(filePicker, App.WindowHandle);

        var file = await filePicker.PickSingleFileAsync();
        if (file != null)
        {
            ViewModel.FilePath = file.Path;
        }
    }

    internal void SaveFile(object _, RoutedEventArgs e)
    { 
    }

    private void OnViewModelPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.FilePath) && this.ViewModel.FilePath is not null)
        {
            ViewModel.SelectedShape = null;
            ViewModel.SelectedSlide = null;
            ViewModel.Slides.Clear();
            ViewModel.Loading = true;

            App.MainWindow.PresentationName = Path.GetFileName(this.ViewModel.FilePath);

            ThreadPool.QueueUserWorkItem(LoadPresentation, this.ViewModel, false);
        }
    }

    private static void LoadPresentation(MainPageViewModel viewModel)
    {
        PresentationDocument presentationDocument = PresentationDocument.Open(viewModel.FilePath, false);

        var presentationPart = presentationDocument.PresentationPart;

        foreach (var slide in presentationPart.SlideParts)
        {
            var model = new SlideModel
            {
                Title = SlideTitle(slide.Slide)
            };

            foreach (var shape in slide.Slide.Descendants<Shape>())
            {
                var shapeModel = new ShapeModel
                {
                    Name = shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Name,
                    NoMove = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoMove ?? false,
                    NoRotation = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoRotation ?? false,
                    NoSelection = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoSelection ?? false,
                    NoTextEdit = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoTextEdit ?? false,
                    NoEditPoints = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoEditPoints ?? false,
                    NoChangeShapeType = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoChangeShapeType ?? false,
                    NoChangeArrowheads = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoChangeArrowheads ?? false,
                    NoAdjustHandles = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoAdjustHandles ?? false,
                    NoResize = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoResize ?? false
                };

                model.Shapes.Add(shapeModel);
            }

            App.MainWindow!.DispatcherQueue.TryEnqueue(() => {
                viewModel.Slides.Add(model); 
            });
        }

        App.MainWindow!.DispatcherQueue.TryEnqueue(() => {
            viewModel.Loading = false;
        });
    }

    private static string SlideTitle(Slide slide)
    {
        var shapes = slide.Descendants<Shape>();

        foreach (var shape in shapes)
        {
            var placeholderShape = shape.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.GetFirstChild<PlaceholderShape>();
            if (placeholderShape != null && placeholderShape.Type != null && placeholderShape.Type.HasValue)
            {
                return (PlaceholderValues)placeholderShape.Type switch
                {
                    PlaceholderValues.Title or PlaceholderValues.CenteredTitle => new string(shape.TextBody?.Descendants<D.Paragraph>().SelectMany(p => p.Descendants<D.Text>().SelectMany(t => t.Text)).ToArray()),
                    _ => string.Empty,
                };
            }
        }

        return string.Empty;
    }
}
