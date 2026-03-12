using System.Globalization;

namespace SlideDesignUnlocker.Mac;

/// <summary>
/// Returns a placeholder string when the bound value is null or whitespace.
/// </summary>
internal class NullOrEmptyToPlaceholderConverter : IValueConverter
{
    public string Placeholder { get; set; } = "(untitled)";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var str = value as string;
        return string.IsNullOrWhiteSpace(str) ? Placeholder : str;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
