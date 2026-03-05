using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Security.Cryptography;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace SlideDesignUnlocker;

public sealed partial class MainPage : Page
{
    internal MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();

        this.ViewModel = new MainPageViewModel();
        this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
    }

    internal async void SelectFile(object _, RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker()
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            FileTypeFilter = { ".pptx" }
        };

        InitializeWithWindow.Initialize(filePicker, App.WindowHandle);

        var file = await filePicker.PickSingleFileAsync();
        if (file is not null)
        {
            this.ViewModel.FileHash = ComputeFileHash(file.Path);
            this.ViewModel.FilePath = file.Path;
        }
    }

    internal void CloseFile(object _, RoutedEventArgs e)
    {
        this.ViewModel.FilePath = null;
        this.ViewModel.FileHash = null;
        this.ViewModel.SelectedSlide = null;
        this.ViewModel.SelectedShape = null;
        this.ViewModel.Slides.Clear();
        this.ViewModel.SlidesChanged = false;
        this.ViewModel.Error = string.Empty;
        this.ViewModel.StatusMessage = string.Empty;
        App.MainWindow.PresentationName = null;
    }

    internal void SaveFile(SplitButton _, SplitButtonClickEventArgs e) => this.SaveFile();

    internal void SaveFile(object _, RoutedEventArgs e) => this.SaveFile();

    private async void SaveFile()
    {
        if (string.IsNullOrWhiteSpace(this.ViewModel.FilePath))
        {
            this.ViewModel.Error = "No presentation is open.";
            return;
        }

        try
        {
            var currentHash = ComputeFileHash(this.ViewModel.FilePath);

            var fileWasModifiedExternally = currentHash != this.ViewModel.FileHash;
            if (fileWasModifiedExternally)
            {
                var dialog = new ContentDialog()
                {
                    XamlRoot = this.XamlRoot,
                    Title = "File Modified Externally",
                    Content = "The file has been modified since you opened it. Do you want to overwrite the changes or save as a new file?",
                    PrimaryButtonText = "Overwrite",
                    SecondaryButtonText = "Save As",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Secondary
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.None)
                {
                    return;
                }

                if (result == ContentDialogResult.Secondary)
                {
                    await this.PickAndSaveFile();
                    return;
                }
            }

            await this.SaveToFile(this.ViewModel.FilePath);
        }
        catch (Exception ex)
        {
            this.ViewModel.Error = $"Failed to save: {ex.Message}";
        }
    }

    internal async void SaveFileAs(object _, RoutedEventArgs e)
    {
        try
        {
            await this.PickAndSaveFile();
        }
        catch (Exception ex)
        {
            this.ViewModel.Error = $"Failed to choose a save location: {ex.Message}";
        }
    }

    private async Task PickAndSaveFile()
    {
        if (string.IsNullOrWhiteSpace(this.ViewModel.FilePath))
        {
            this.ViewModel.Error = "No presentation is open.";
            return;
        }

        var filePicker = new FileSavePicker()
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"{Path.GetFileNameWithoutExtension(this.ViewModel.FilePath)}.Fixed",
            DefaultFileExtension = ".pptx"
        };
        filePicker.FileTypeChoices.Add("PowerPoint Presentation", new List<string> { ".pptx" });

        InitializeWithWindow.Initialize(filePicker, App.WindowHandle);

        var file = await filePicker.PickSaveFileAsync();
        if (file is not null && !string.IsNullOrWhiteSpace(file.Path))
        {
            await this.SaveToFile(file.Path);
        }
    }

    private async Task SaveToFile(string targetPath)
    {
        var sourcePath = this.ViewModel.FilePath!;
        var savingToSameFile = string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase);
        var workingPath = savingToSameFile
            ? Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}")
            : targetPath;

        this.ViewModel.Saving = true;
        this.ViewModel.StatusMessage = string.Empty;
        this.ViewModel.Error = string.Empty;

        try
        {
            File.Copy(sourcePath, workingPath, overwrite: true);

            PresentationService.SaveChangesToPresentation(workingPath, this.ViewModel.Slides);

            if (savingToSameFile)
            {
                File.Copy(workingPath, targetPath, overwrite: true);
                File.Delete(workingPath);
            }

            this.ViewModel.FileHash = ComputeFileHash(targetPath);

            if (!savingToSameFile)
            {
                this.ViewModel.FilePath = targetPath;
            }
            else
            {
                this.ReloadPresentation();
            }

            this.ShowStatusMessage("File saved successfully.");
        }
        catch (Exception ex)
        {
            this.ViewModel.Error = $"Failed to save: {ex.Message}";

            if (savingToSameFile && File.Exists(workingPath))
            {
                try { File.Delete(workingPath); } catch { }
            }
        }
        finally
        {
            this.ViewModel.Saving = false;
        }

        await Task.CompletedTask;
    }

    internal async void OpenInPowerPoint(object _, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(this.ViewModel.FilePath))
        {
            return;
        }

        if (this.ViewModel.SlidesChanged)
        {
            var dialog = new ContentDialog()
            {
                XamlRoot = this.XamlRoot,
                Title = "Unsaved Changes",
                Content = "You have unsaved changes. Do you want to save before opening in PowerPoint, or open the current file without your changes?",
                PrimaryButtonText = "Save and Open",
                SecondaryButtonText = "Open Without Saving",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.None)
            {
                return;
            }

            if (result == ContentDialogResult.Primary)
            {
                this.SaveFile();

                // If save failed or was cancelled, don't open
                if (this.ViewModel.SlidesChanged)
                {
                    return;
                }
            }
        }

        try
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(this.ViewModel.FilePath);
            await Windows.System.Launcher.LaunchFileAsync(file);
        }
        catch (Exception ex)
        {
            this.ViewModel.Error = $"Failed to open in PowerPoint: {ex.Message}";
        }
    }

    internal async void ShowAbout(object _, RoutedEventArgs e)
    {
        var aboutDialog = new AboutDialog { XamlRoot = this.XamlRoot };

        await aboutDialog.ShowAsync();
    }

    private void OnViewModelPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.ViewModel.FilePath) && this.ViewModel.FilePath is not null)
        {
            this.ReloadPresentation();
        }
    }

    private void ReloadPresentation()
    {
        this.ViewModel.SelectedShape = null;
        this.ViewModel.SelectedSlide = null;
        this.ViewModel.Slides.Clear();
        this.ViewModel.Loading = true;

        App.MainWindow.PresentationName = Path.GetFileName(this.ViewModel.FilePath);

        ThreadPool.QueueUserWorkItem(PresentationService.LoadPresentation, this.ViewModel, false);
    }

    private static string? ComputeFileHash(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var hash = SHA256.HashData(stream);
            return Convert.ToHexString(hash);
        }
        catch
        {
            return null;
        }
    }

    private async void ShowStatusMessage(string message, int autoHideDelayMs = 3000)
    {
        this.ViewModel.StatusMessage = message;
        await Task.Delay(autoHideDelayMs);

        if (this.ViewModel.StatusMessage == message)
        {
            this.ViewModel.StatusMessage = string.Empty;
        }
    }
}
