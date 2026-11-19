
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel : BaseViewModel
{
    private static readonly System.Resources.ResourceManager Resources = new(
        "AcademiaDoZe.Presentation.AppMaui.Resources.Strings.AppResources",
        typeof(ColaboradorListViewModel).Assembly);

    private static string T(string key) =>
        Resources.GetString(key, System.Globalization.CultureInfo.CurrentUICulture) ?? key;

    private readonly IColaboradorService _colaboradorService;

    public ObservableCollection<string> FilterTypes { get; } =
        [T("strNome"), "CPF", "Id", "Email", T("strTipo"), T("strVinculo")];

    private ObservableCollection<ColaboradorDto> _colaboradores = [];
    public ObservableCollection<ColaboradorDto> Colaboradores
    {
        get => _colaboradores;
        set => SetProperty(ref _colaboradores, value);
    }

    private ColaboradorDto? _selectedColaborador;
    public ColaboradorDto? SelectedColaborador
    {
        get => _selectedColaborador;
        set => SetProperty(ref _selectedColaborador, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = T("strNome");
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public ColaboradorListViewModel(IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
        Title = T("listMsg012");
    }

    [RelayCommand]
    private async Task AddColaboradorAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("colaborador");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("listMsg001"), T("listMsg013").Replace("{ex.Message}", ex.Message), "OK");
        }
    }

    [RelayCommand]
    private async Task EditColaboradorAsync(ColaboradorDto colaborador)
    {
        if (colaborador == null) return;

        try
        {
            await Shell.Current.GoToAsync($"colaborador?Id={colaborador.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("listMsg001"), T("listMsg014").Replace("{ex.Message}", ex.Message), "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadColaboradoresAsync();
    }

    [RelayCommand]
    private async Task SearchColaboradoresAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Colaboradores.Clear();

            IEnumerable<ColaboradorDto> resultados = [];
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _colaboradorService.ObterTodosAsync(cts.Token) ?? [];
            }
            else if (SelectedFilterType == FilterTypes[0])
            {
                var todos = await _colaboradorService.ObterTodosAsync(cts.Token) ?? [];
                resultados = todos.Where(c =>
                    c.Nome.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            else if (SelectedFilterType == "CPF")
            {
                var cpf = new string(SearchText.Where(char.IsDigit).ToArray());

                resultados = await _colaboradorService
                    .BuscarPorCpfParcialAsync(cpf, cts.Token);
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(SearchText.Trim(), out int id) || id <= 0)
                {
                    await Shell.Current.DisplayAlertAsync(T("listMsg015"), T("listMsg016"), "OK");
                    return;
                }

                var colaborador = await _colaboradorService.ObterPorIdAsync(id, cts.Token);
                if (colaborador != null) resultados = [colaborador];
            }
            else if (SelectedFilterType == "Email")
            {
                var colaborador = await _colaboradorService.ObterPorEmailAsync(SearchText.Trim(), cts.Token);
                if (colaborador != null) resultados = [colaborador];
            }
            else if (SelectedFilterType == FilterTypes[4])
            {
                var tipos = Enum.GetValues<AppColaboradorTipo>()
                    .Where(t => t.ToString().Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)
                        || t.GetDisplayName().Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase));

                var lista = new List<ColaboradorDto>();

                foreach (var tipo in tipos)
                {
                    var itens = await _colaboradorService.ObterPorTipoAsync(tipo, cts.Token);
                    if (itens != null) lista.AddRange(itens);
                }

                resultados = lista.DistinctBy(c => c.Id);
            }
            else if (SelectedFilterType == FilterTypes[5])
            {
                var vinculos = Enum.GetValues<AppColaboradorVinculo>()
                    .Where(v => v.ToString().Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)
                        || v.GetDisplayName().Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase));

                var lista = new List<ColaboradorDto>();

                foreach (var vinculo in vinculos)
                {
                    var itens = await _colaboradorService.ObterPorVinculoAsync(vinculo, cts.Token);
                    if (itens != null) lista.AddRange(itens);
                }

                resultados = lista.DistinctBy(c => c.Id);
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in resultados)
                    Colaboradores.Add(item);
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(T("listMsg017"), T("listMsg018"), "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("listMsg001"), T("listMsg019").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadColaboradoresAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Colaboradores.Clear();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradores = await _colaboradorService.ObterTodosAsync(cts.Token);

            if (colaboradores != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var colaborador in colaboradores)
                        Colaboradores.Add(colaborador);
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(T("listMsg017"), T("listMsg020"), "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("listMsg001"), T("listMsg021").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteColaboradorAsync(ColaboradorDto colaborador)
    {
        if (colaborador == null) return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            T("listMsg004"),
            T("listMsg022").Replace("{colaborador.Nome}", colaborador.Nome),
            T("listMsg006"), T("listMsg007"));

        if (!confirmar) return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool sucesso = await _colaboradorService.RemoverAsync(colaborador.Id, cts.Token);

            if (sucesso)
            {
                Colaboradores.Remove(colaborador);
                await Shell.Current.DisplayAlertAsync(T("listMsg008"), T("listMsg023"), "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(T("listMsg001"), T("listMsg024"), "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("listMsg001"), T("listMsg025").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
