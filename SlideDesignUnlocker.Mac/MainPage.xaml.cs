using System.Security.Cryptography;
using SlideDesignUnlocker;

namespace SlideDesignUnlocker.Mac;

public partial class MainPage : ContentPage
{
    internal MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();

        this.ViewModel = new MainPageViewModel();
        this.BindingContext = this.ViewModel;
    }

    internal async void SelectFile(object? sender, EventArgs e)
    {
        try
        {
            var pptxType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.MacCatalyst, ["org.openxmlformats.presentationml.presentation"] },
                { DevicePlatform.iOS, ["org.openxmlformats.presentationml.presentation"] }
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a PowerPoint presentation",
                FileTypes = pptxType
            });

            if (result is null)
            {
                return;
            }

            // Copy to the app cache directory for sandbox-safe OpenXml file access
            var workingPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}{Path.GetExtension(result.FullPath)}");
            File.Copy(result.FullPath, workingPath, overwrite: true);

            this.ViewModel.OriginalFilePath = result.FullPath;
            this.ViewModel.WorkingFilePath = workingPath;
            this.ViewModel.FileHash = ComputeFileHash(result.FullPath);
            this.ViewModel.FilePath = Path.GetFileName(result.FullPath);
            this.ViewModel.Error = null;

            this.UpdateWindowTitle(this.ViewModel.FilePath);
            this.LoadPresentation(workingPath);
        }
        catch (Exception ex)
        {
            this.ViewModel.Error = $"Failed to open file: {ex.Message}";
        }
    }

    internal void CloseFile(object? sender, EventArgs e)
    {
        this.CleanupWorkingFile();

        this.ViewModel.FilePath = null;
        this.ViewModel.OriginalFilePath = null;
        this.ViewModel.WorkingFilePath = null;
        this.ViewModel.FileHash = null;
        this.ViewModel.SelectedSlide = null;
        this.ViewModel.SelectedShape = null;
        this.ViewModel.Slides.Clear();
        this.ViewModel.SlidesChanged = false;
        this.ViewModel.Error = null;
        this.ViewModel.StatusMessage = null;

        this.UpdateWindowTitle(null);
    }

    internal async void SaveFile(object? sender, EventArgs e)
    {
        if (this.ViewModel.WorkingFilePath is null || this.ViewModel.OriginalFilePath is null)
        {
            return;
        }

        if (!this.ViewModel.SlidesChanged)
        {
            return;
        }

        try
        {
            var currentHash = ComputeFileHash(this.ViewModel.OriginalFilePath);
            if (currentHash != this.ViewModel.FileHash)
            {
                var overwrite = await this.DisplayAlertAsync(
                    "File Modified Externally",
                    "The file has been modified since you opened it. Do you want to overwrite the changes?",
                    "Overwrite",
                    "Cancel");

                if (!overwrite)
                {
                    return;
                }
            }

            this.ViewModel.Saving = true;
            this.ViewModel.Error = null;
            this.ViewModel.StatusMessage = null;

            PresentationService.SaveChangesToPresentation(this.ViewModel.WorkingFilePath, this.ViewModel.Slides);
            File.Copy(this.ViewModel.WorkingFilePath, this.ViewModel.OriginalFilePath, overwrite: true);

            this.ViewModel.FileHash = ComputeFileHash(this.ViewModel.OriginalFilePath);

            // Reload the working copy from the saved file to reset change tracking
            File.Copy(this.ViewModel.OriginalFilePath, this.ViewModel.WorkingFilePath, overwrite: true);
            this.LoadPresentation(this.ViewModel.WorkingFilePath);

            this.ShowStatusMessage("File saved successfully.");
        }
        catch (Exception ex)
        {
            this.ViewModel.Error = $"Failed to save: {ex.Message}";
        }
        finally
        {
            this.ViewModel.Saving = false;
        }
    }

    internal async void ShowAbout(object? sender, EventArgs e)
    {
        await this.DisplayAlertAsync(
            "Slide Design Unlocker",
            "Inspect and modify PowerPoint element locks.",
            "OK");
    }

    private void LoadPresentation(string filePath)
    {
        this.ViewModel.SelectedShape = null;
        this.ViewModel.SelectedSlide = null;
        this.ViewModel.Slides.Clear();
        this.ViewModel.Loading = true;
        this.ViewModel.Error = null;

        Task.Run(() =>
        {
            var slides = PresentationService.LoadSlides(filePath);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (slides is null)
                {
                    this.ViewModel.Error = "Could not parse this presentation correctly.";
                    this.ViewModel.Loading = false;
                    return;
                }

                foreach (var slide in slides)
                {
                    this.ViewModel.Slides.Add(slide);
                }

                this.ViewModel.Loading = false;
            });
        });
    }

    private async void ShowStatusMessage(string message)
    {
        this.ViewModel.StatusMessage = message;

        await Task.Delay(3000);

        if (this.ViewModel.StatusMessage == message)
        {
            this.ViewModel.StatusMessage = null;
        }
    }

    private void UpdateWindowTitle(string? fileName)
    {
        if (Application.Current?.Windows is { Count: > 0 } windows && windows[0] is Window window)
        {
            window.Title = string.IsNullOrEmpty(fileName) ? "Slide Design Unlocker" : $"{fileName} — Slide Design Unlocker";
        }
    }

    private void CleanupWorkingFile()
    {
        if (this.ViewModel.WorkingFilePath is not null && File.Exists(this.ViewModel.WorkingFilePath))
        {
            try { File.Delete(this.ViewModel.WorkingFilePath); }
            catch { /* best-effort cleanup */ }
        }
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
}
