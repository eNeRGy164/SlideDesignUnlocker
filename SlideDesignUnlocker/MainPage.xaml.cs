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
        var currentHash = ComputeFileHash(this.ViewModel.FilePath!);

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

        await this.SaveToFile(this.ViewModel.FilePath!);
    }

    internal async void SaveFileAs(object _, RoutedEventArgs e)
    {
        await this.PickAndSaveFile();
    }

    private async Task PickAndSaveFile()
    {
        var filePicker = new FileSavePicker()
        {
            SuggestedFileName = $"{Path.GetFileNameWithoutExtension(this.ViewModel.FilePath)}.Fixed{Path.GetExtension(this.ViewModel.FilePath)}",
            FileTypeChoices =
            {
                { "PowerPoint Presentation", [".pptx"] }
            }
        };

        InitializeWithWindow.Initialize(filePicker, App.WindowHandle);

        var file = await filePicker.PickSaveFileAsync();
        if (file is not null)
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
