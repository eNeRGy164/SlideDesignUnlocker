using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using D = DocumentFormat.OpenXml.Drawing;

namespace SlideDesignUnlocker;

internal static class PresentationService
{
    internal static void LoadPresentation(MainPageViewModel viewModel)
    {
        using var presentationDocument = PresentationDocument.Open(viewModel.FilePath!, false);
        if (presentationDocument.PresentationPart is null)
        {
            viewModel.Error = "Could not parse this presentation correctly";
            return;
        }

        var presentationPart = presentationDocument.PresentationPart;
        var presentation = presentationPart.Presentation;

        if (presentation?.SlideIdList is not null)
        {
            foreach (var slideId in presentation.SlideIdList.Elements<SlideId>())
            {
                if (presentationPart.GetPartById(slideId.RelationshipId!) is not SlidePart slide || slide.Slide is null)
                {
                    continue;
                }

                var model = new SlideModel
                {
                    Title = GetSlideTitle(slide.Slide!),
                    SlideRelationshipId = slideId.RelationshipId!
                };

                foreach (var shape in slide.Slide.Descendants<Shape>())
                {
                    var shapeModel = new ShapeModel
                    {
                        ShapeId = shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Id ?? 0,
                        Name = shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Name,
                        NoMove = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoMove ?? false,
                        NoRotation = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoRotation ?? false,
                        NoTextEdit = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoTextEdit ?? false,
                        NoEditPoints = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoEditPoints ?? false,
                        NoChangeShapeType = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoChangeShapeType ?? false,
                        NoChangeArrowheads = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoChangeArrowheads ?? false,
                        NoAdjustHandles = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoAdjustHandles ?? false,
                        NoResize = shape.NonVisualShapeProperties?.NonVisualShapeDrawingProperties?.ShapeLocks?.NoResize ?? false,
                        IsDesignElement = shape.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.Descendants<OpenXmlUnknownElement>().Any(e => e.LocalName == "designElem" && e.HasAttributes && e.GetAttribute("val", default!).Value == "1") ?? false
                    };

                    shapeModel.CaptureInitialState();
                    model.Shapes.Insert(0, shapeModel);
                }

                App.MainWindow.DispatcherQueue.TryEnqueue(() => viewModel.Slides.Add(model));
            }
        }

        App.MainWindow.DispatcherQueue.TryEnqueue(() => { viewModel.Loading = false; });
    }

    internal static void SaveChangesToPresentation(string filePath, IEnumerable<SlideModel> slides)
    {
        using var presentationDocument = PresentationDocument.Open(filePath, isEditable: true);
        if (presentationDocument.PresentationPart is null)
        {
            throw new InvalidOperationException("Presentation structure is invalid");
        }

        var presentationPart = presentationDocument.PresentationPart;

        var changedShapes = slides
            .Where(s => s.HasElementsWithChanges)
            .SelectMany(s => s.Shapes
                .Where(shape => shape.HasChanges)
                .Select(shape => (SlideRelationshipId: s.SlideRelationshipId!, Shape: shape)))
            .ToList();

        foreach (var (slideRelationshipId, shapeModel) in changedShapes)
        {
            if (presentationPart.GetPartById(slideRelationshipId) is not SlidePart slidePart || slidePart.Slide is null)
            {
                continue;
            }

            var shape = slidePart.Slide.Descendants<Shape>()
                .FirstOrDefault(s => s.NonVisualShapeProperties?.NonVisualDrawingProperties?.Id?.Value == shapeModel.ShapeId);

            if (shape?.NonVisualShapeProperties?.NonVisualShapeDrawingProperties is null)
            {
                continue;
            }

            ApplyShapeChanges(shape, shapeModel);
        }

        presentationDocument.Save();
    }

    private static void ApplyShapeChanges(Shape shape, ShapeModel shapeModel)
    {
        var drawingProps = shape.NonVisualShapeProperties!.NonVisualShapeDrawingProperties!;

        drawingProps.ShapeLocks ??= new D.ShapeLocks();
        var locks = drawingProps.ShapeLocks;

        locks.NoMove = shapeModel.NoMove ? true : null;
        locks.NoResize = shapeModel.NoResize ? true : null;
        locks.NoRotation = shapeModel.NoRotation ? true : null;
        locks.NoEditPoints = shapeModel.NoEditPoints ? true : null;
        locks.NoChangeShapeType = shapeModel.NoChangeShapeType ? true : null;
        locks.NoChangeArrowheads = shapeModel.NoChangeArrowheads ? true : null;
        locks.NoAdjustHandles = shapeModel.NoAdjustHandles ? true : null;
        locks.NoTextEdit = shapeModel.NoTextEdit ? true : null;

        var appProps = shape.NonVisualShapeProperties!.ApplicationNonVisualDrawingProperties;
        if (appProps is not null)
        {
            var existingDesignElem = appProps.Descendants<OpenXmlUnknownElement>()
                .FirstOrDefault(e => e.LocalName == "designElem");
            existingDesignElem?.Remove();

            if (shapeModel.IsDesignElement)
            {
                var designElem = new OpenXmlUnknownElement("p14", "designElem", "http://schemas.microsoft.com/office/powerpoint/2010/main");
                designElem.SetAttribute(new OpenXmlAttribute("val", string.Empty, "1"));
                appProps.AppendChild(designElem);
            }
        }
    }

    private static string GetSlideTitle(Slide slide)
    {
        var title = string.Empty;

        var shapes = slide.Descendants<Shape>();
        foreach (var shape in shapes)
        {
            var placeholderShape = shape.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.GetFirstChild<PlaceholderShape>();
            if (placeholderShape is { Type.HasValue: true })
            {
                var type = placeholderShape.Type.Value;
                if (type == PlaceholderValues.Title || type == PlaceholderValues.CenteredTitle)
                {
                    title = new string(shape.TextBody?.Descendants<D.Paragraph>().SelectMany(p => p.Descendants<D.Text>().SelectMany(t => t.Text)).ToArray());
                }
            }
        }

        return title;
    }
}
