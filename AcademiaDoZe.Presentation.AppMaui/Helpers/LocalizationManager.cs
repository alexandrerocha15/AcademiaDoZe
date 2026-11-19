using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

public sealed class LocalizationManager : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationManager> _instance =
        new(() => new LocalizationManager());

    public static LocalizationManager Instance => _instance.Value;

    private readonly ResourceManager _resources = new(
        "AcademiaDoZe.Presentation.AppMaui.Resources.Strings.AppResources",
        typeof(LocalizationManager).Assembly);

    public event PropertyChangedEventHandler? PropertyChanged;

    private LocalizationManager()
    {
    }

    public string this[string key] =>
        _resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public string FormatoDataCurta =>
        CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

    public string FormatoDataLonga =>
        CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;

    public void AlterarCultura(string codigo)
    {
        var cultura = CultureInfo.GetCultureInfo(codigo);

        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;

        CultureInfo.CurrentCulture = cultura;
        CultureInfo.CurrentUICulture = cultura;

//#if WINDOWS
//        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride =
//            cultura.Name;
//#endif

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(null));
    }
}
