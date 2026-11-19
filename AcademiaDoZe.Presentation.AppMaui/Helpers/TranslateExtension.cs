using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

[ContentProperty(nameof(Key))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding(
            $"[{Key}]",
            source: LocalizationManager.Instance,
            mode: BindingMode.OneWay);
    }

    object IMarkupExtension.ProvideValue(
        IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
