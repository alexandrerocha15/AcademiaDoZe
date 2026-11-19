using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    public ObservableCollection<string> FilterTypes { get; } =
        ["Cidade", "Id", "Cep"];

    private ObservableCollection<LogradouroDto> _logradouros = [];

    public ObservableCollection<LogradouroDto> Logradouros
    {
        get => _logradouros;
        set => SetProperty(ref _logradouros, value);
    }

    private LogradouroDto? _selectedLogradouro;

    public LogradouroDto? SelectedLogradouro
    {
        get => _selectedLogradouro;
        set => SetProperty(ref _selectedLogradouro, value);
    }

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Cidade";

    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = LocalizationManager.Instance["strLogradouro"];
    }

    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("logradouro");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"],
                string.Format(LocalizationManager.Instance["strErroAoNavegarParaTelaDe"], ex.Message),
                "OK");
        }
    }

    [RelayCommand]
    private async Task EditLogradouroAsync(LogradouroDto logradouro)
    {
        try
        {
            if (logradouro == null)
                return;

            await Shell.Current.GoToAsync($"logradouro?Id={logradouro.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                string.Format(LocalizationManager.Instance["strErroAoNavegarParaTelaDe055"], ex.Message),
                "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadLogradourosAsync();
    }

    [RelayCommand]
    private async Task SearchLogradourosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Logradouros.Clear();
            });

            IEnumerable<LogradouroDto> resultados = [];

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados =
                    await _logradouroService.ObterTodosAsync(cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Cidade")
            {
                resultados =
                    await _logradouroService.ObterPorCidadeAsync(
                        SearchText.Trim(),
                        cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(SearchText.Trim(), out int id) || id <= 0)
                {
                    await Shell.Current.DisplayAlertAsync(
                        LocalizationManager.Instance["listMsg015"],
                        LocalizationManager.Instance["strDigiteONomeDaRua"],
                        "OK");

                    return;
                }

                var logradouro =
                    await _logradouroService.ObterPorIdAsync(id, cts.Token);

                if (logradouro != null)
                    resultados = [logradouro];
            }
            else if (SelectedFilterType == "Cep")
            {
                var cepLimpo =
                    new string([.. SearchText.Where(char.IsDigit)]);

                if (cepLimpo.Length != 8)
                {
                    await Shell.Current.DisplayAlertAsync(
                        LocalizationManager.Instance["listMsg015"],
                        LocalizationManager.Instance["strDigiteOBairro"],
                        "OK");

                    return;
                }

                var logradouro =
                    await _logradouroService.ObterPorCepAsync(
                        cepLimpo,
                        cts.Token);

                if (logradouro != null)
                    resultados = [logradouro];
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in resultados)
                {
                    Logradouros.Add(item);
                }

                OnPropertyChanged(nameof(Logradouros));
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg017"],
                LocalizationManager.Instance["strDigiteACidade"],
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"],
                string.Format(LocalizationManager.Instance["strErroAoBuscarLogradouros0"], ex.Message),
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadLogradourosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Logradouros.Clear();
                OnPropertyChanged(nameof(Logradouros));
            });

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var logradourosList =
                await _logradouroService.ObterTodosAsync(cts.Token);

            if (logradourosList != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var logradouro in logradourosList)
                    {
                        Logradouros.Add(logradouro);
                    }

                    OnPropertyChanged(nameof(Logradouros));
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg017"],
                LocalizationManager.Instance["strDigiteOEstado"],
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"],
                string.Format(LocalizationManager.Instance["strErroAoCarregarLogradouros0"], ex.Message),
                "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteLogradouroAsync(LogradouroDto logradouro)
    {
        if (logradouro == null)
            return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            LocalizationManager.Instance["listMsg004"],
            string.Format(LocalizationManager.Instance["strDesejaRealmenteExcluirOLogradouro0"], logradouro.Nome),
            "Sim",
            "Não");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            bool success =
                await _logradouroService.RemoverAsync(
                    logradouro.Id,
                    cts.Token);

            if (success)
            {
                Logradouros.Remove(logradouro);

                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["listMsg008"],
                    LocalizationManager.Instance["strDigiteOPais"],
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["listMsg001"],
                    LocalizationManager.Instance["strBuscarMatricula"],
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg017"],
                LocalizationManager.Instance["strNenhumaMatriculaEncontrada"],
                "OK");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains(
                    "FOREIGN KEY",
                    StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains(
                    "constraint",
                    StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains(
                    "REFERENCE",
                    StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["strNovaMatricula"],
                    LocalizationManager.Instance["strAlunoDaMatricula"],
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["listMsg001"],
                    string.Format(LocalizationManager.Instance["strErroAoExcluirLogradouro0"], ex.Message),
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}