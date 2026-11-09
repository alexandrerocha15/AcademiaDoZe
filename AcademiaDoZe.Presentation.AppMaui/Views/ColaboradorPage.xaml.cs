using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorPage : ContentPage
{
    public ColaboradorPage(ColaboradorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ColaboradorViewModel viewModel)
            await viewModel.InitializeAsync();
    }

    private void OnShowPasswordToggled(object? sender, ToggledEventArgs e)
    {
        SenhaEntry.IsPassword = !e.Value;
    }

    private void OnEmailUnfocused(object? sender, FocusEventArgs e)
    {
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            EmailErrorLabel.IsVisible = false;
            return;
        }

        bool valido = Regex.IsMatch(
            email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);

        EmailErrorLabel.IsVisible = !valido;
    }
}