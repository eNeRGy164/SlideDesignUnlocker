using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using D = DocumentFormat.OpenXml.Drawing;
using P16 = DocumentFormat.OpenXml.Office2016.Presentation;

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
                        IsDesignElement = IsDesignElement(shape.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties)
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
            // Remove legacy p14:designElem if present
            var legacyDesignElem = appProps.Descendants<OpenXmlUnknownElement>()
                .FirstOrDefault(e => e.LocalName == "designElem");
            legacyDesignElem?.Remove();

            // Remove typed p16:designElem if present
            var typedDesignElem = appProps.GetFirstChild<P16.DesignElement>();
            typedDesignElem?.Remove();

            if (shapeModel.IsDesignElement)
            {
                appProps.AppendChild(new P16.DesignElement { Val = true });
            }
        }
    }

    private static bool IsDesignElement(ApplicationNonVisualDrawingProperties? appProps)
    {
        if (appProps is null)
        {
            return false;
        }

        // Check typed p16:designElem
        var typed = appProps.GetFirstChild<P16.DesignElement>();
        if (typed?.Val is not null)
        {
            return typed.Val;
        }

        // Fall back to legacy p14:designElem (older PowerPoint versions)
        var legacy = appProps.Descendants<OpenXmlUnknownElement>()
            .FirstOrDefault(e => e.LocalName == "designElem");
        return legacy is not null && legacy.GetAttribute("val", string.Empty).Value == "1";
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
