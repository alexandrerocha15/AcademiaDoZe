using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoListPage : ContentPage
{
    private CancellationTokenSource? _debounceCts;

    public AlunoListPage(AlunoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AlunoListViewModel viewModel)
            await viewModel.LoadAlunosCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        _debounceCts?.Cancel();
        base.OnDisappearing();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        AgendarPesquisa();
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        AgendarPesquisa();
    }

    private async void AgendarPesquisa()
    {
        _debounceCts?.Cancel();

        var cts = new CancellationTokenSource();
        _debounceCts = cts;

        try
        {
            await Task.Delay(300, cts.Token);

            if (cts.IsCancellationRequested)
                return;

            if (BindingContext is AlunoListViewModel viewModel)
                await viewModel.SearchAlunosCommand.ExecuteAsync(null);
        }
        catch (OperationCanceledException)
        {
            // Uma nova digitação cancelou esta pesquisa.
        }
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is AlunoDto aluno &&
            BindingContext is AlunoListViewModel viewModel)
        {
            await viewModel.EditAlunoCommand.ExecuteAsync(aluno);
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is AlunoDto aluno &&
            BindingContext is AlunoListViewModel viewModel)
        {
            await viewModel.DeleteAlunoCommand.ExecuteAsync(aluno);
        }
    }
}