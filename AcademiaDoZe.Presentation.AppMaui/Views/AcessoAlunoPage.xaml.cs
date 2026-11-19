using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoAlunoPage : ContentPage
{
    public AcessoAlunoPage(AcessoAlunoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AcessoAlunoViewModel viewModel)
            await viewModel.CarregarCommand.ExecuteAsync(null);
    }
}
