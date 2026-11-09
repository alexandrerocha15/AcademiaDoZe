using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    private CancellationTokenSource? _debounceCts;

    public ColaboradorListPage(ColaboradorListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ColaboradorListViewModel viewModel)
            await viewModel.LoadColaboradoresCommand.ExecuteAsync(null);
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
        _debounceCts?.Dispose();

        var cts = new CancellationTokenSource();
        _debounceCts = cts;

        try
        {
            await Task.Delay(300, cts.Token);

            if (cts.IsCancellationRequested)
                return;

            if (BindingContext is ColaboradorListViewModel viewModel)
                await viewModel.SearchColaboradoresCommand.ExecuteAsync(null);
        }
        catch (OperationCanceledException)
        {
            // Nova digitação cancelou a pesquisa anterior.
        }
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is ColaboradorDto colaborador &&
            BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.EditColaboradorCommand.ExecuteAsync(colaborador);
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is ColaboradorDto colaborador &&
            BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.DeleteColaboradorCommand.ExecuteAsync(colaborador);
        }
    }
}