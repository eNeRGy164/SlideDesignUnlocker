﻿using DocumentFormat.OpenXml.Packaging;
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
        this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
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
            this.ViewModel.FilePath = file.Path;
        }
    }

    internal void SaveFile(object _, RoutedEventArgs e)
    {
    }

    internal async void ShowAbout(object _, RoutedEventArgs e)
    {
        var aboutDialog = new AboutDialog
        {
            XamlRoot = this.XamlRoot
        };

        await aboutDialog.ShowAsync();
    }

    private void OnViewModelPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.ViewModel.FilePath) && this.ViewModel.FilePath is not null)
        {
            this.ViewModel.SelectedShape = null;
            this.ViewModel.SelectedSlide = null;
            this.ViewModel.Slides.Clear();
            this.ViewModel.Loading = true;

            App.MainWindow.PresentationName = Path.GetFileName(this.ViewModel.FilePath);

            ThreadPool.QueueUserWorkItem(LoadPresentation, this.ViewModel, false);
        }
    }

    private static void LoadPresentation(MainPageViewModel viewModel)
    {
        var presentationDocument = PresentationDocument.Open(viewModel.FilePath!, false);
        if (presentationDocument.PresentationPart is null)
        {
            viewModel.Error = "Could not parse this presentation correctly";
            return;
        }

        var presentationPart = presentationDocument.PresentationPart;
        var presentation = presentationPart.Presentation;

        if (presentation.SlideIdList != null)
        {
            // Get the title of each slide in the slide order.
            foreach (var slideId in presentation.SlideIdList.Elements<SlideId>())
            {
                if (presentationPart.GetPartById(slideId.RelationshipId!) is not SlidePart slide)
                {
                    continue;
                }

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


                App.MainWindow.DispatcherQueue.TryEnqueue(() => { viewModel.Slides.Add(model); });
            }
        }

        App.MainWindow.DispatcherQueue.TryEnqueue(() => { viewModel.Loading = false; });
    }

    private static string SlideTitle(Slide slide)
    {
        var shapes = slide.Descendants<Shape>();

        var title = string.Empty;

        foreach (var shape in shapes)
        {
            var placeholderShape = shape.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.GetFirstChild<PlaceholderShape>();
            if (placeholderShape != null && placeholderShape.Type != null && placeholderShape.Type.HasValue)
            {
                title = (PlaceholderValues)placeholderShape.Type switch
                {
                    PlaceholderValues.Title or PlaceholderValues.CenteredTitle => new string(shape.TextBody?.Descendants<D.Paragraph>().SelectMany(p => p.Descendants<D.Text>().SelectMany(t => t.Text)).ToArray()),
                    _ => title,
                };
            }
        }

        return title;
    }
}
