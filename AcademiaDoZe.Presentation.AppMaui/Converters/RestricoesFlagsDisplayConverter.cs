using AcademiaDoZe.Application.Enums;
using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class RestricoesFlagsDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AppMatriculaRestricoes flags)
            return string.Empty;

        if (flags == AppMatriculaRestricoes.None)
            return flags.GetDisplayName();

        return string.Join(", ",
            Enum.GetValues<AppMatriculaRestricoes>()
                .Where(r => r != AppMatriculaRestricoes.None && flags.HasFlag(r))
                .Select(r => r.GetDisplayName()));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}