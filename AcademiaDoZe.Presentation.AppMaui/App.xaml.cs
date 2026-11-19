using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
        Resources["LocalizedStrings"] = LocalizationManager.Instance;

        WeakReferenceMessenger.Default.Register<CulturaPreferencesUpdatedMessage>(
            this,
            (recipient, message) =>
            {
                LocalizationManager.Instance.AlterarCultura(message.Value);
                Preferences.Set("Cultura", message.Value);
            });

        // Aplicar o tema salvo nas preferências
        AplicarTema();

        // Escutar alterações do tema
        WeakReferenceMessenger.Default.Register<TemaPreferencesUpdatedMessage>(
            this,
            (r, m) =>
            {
                AplicarTema();
            });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private void AplicarTema()
    {
        UserAppTheme = Preferences.Get("Tema", "system") switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }
}