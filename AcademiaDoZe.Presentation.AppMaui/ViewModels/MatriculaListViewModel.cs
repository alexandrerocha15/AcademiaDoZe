using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;

    public ObservableCollection<string> FilterTypes { get; } =
        ["Id", "Id Aluno", "Ativas", "Vencendo em dias"];

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Ativas";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public ObservableCollection<MatriculaDto> Matriculas { get; } = [];

    public MatriculaListViewModel(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
        Title = "Matrículas";
    }

    [RelayCommand]
    private async Task AddMatriculaAsync()
    {
        await Shell.Current.GoToAsync("matricula");
    }

    [RelayCommand]
    private async Task EditMatriculaAsync(MatriculaDto matricula)
    {
        if (matricula is null)
            return;

        await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
    }

    [RelayCommand]
    private async Task LoadMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var resultados = await _matriculaService.ObterTodasAsync(cts.Token);
            AtualizarLista(resultados);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro",
                $"Erro ao carregar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadMatriculasAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task SearchMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            IEnumerable<MatriculaDto> resultados = [];
            var texto = SearchText?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(texto))
            {
                resultados = SelectedFilterType == "Ativas"
                    ? await _matriculaService.ObterAtivasAsync(0, cts.Token)
                    : await _matriculaService.ObterTodasAsync(cts.Token);
            }
            else if (SelectedFilterType == "Id" &&
                     int.TryParse(texto, out int id))
            {
                var matricula = await _matriculaService.ObterPorIdAsync(id, cts.Token);
                if (matricula is not null)
                    resultados = [matricula];
            }
            else if (SelectedFilterType == "Id Aluno" &&
                     int.TryParse(texto, out int alunoId))
            {
                resultados = await _matriculaService.ObterPorAlunoIdAsync(alunoId, cts.Token);
            }
            else if (SelectedFilterType == "Ativas" &&
                     int.TryParse(texto, out int idAlunoAtivo))
            {
                resultados = await _matriculaService.ObterAtivasAsync(idAlunoAtivo, cts.Token);
            }
            else if (SelectedFilterType == "Vencendo em dias" &&
                     int.TryParse(texto, out int dias))
            {
                resultados = await _matriculaService.ObterVencendoEmDiasAsync(dias, cts.Token);
            }

            AtualizarLista(resultados);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro",
                $"Erro ao pesquisar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMatriculaAsync(MatriculaDto matricula)
    {
        if (matricula is null)
            return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Excluir matrícula",
            $"Deseja excluir a matrícula {matricula.Id}?",
            "Sim", "Não");

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            if (await _matriculaService.RemoverAsync(matricula.Id, cts.Token))
            {
                Matriculas.Remove(matricula);
                await Shell.Current.DisplayAlertAsync(
                    "Sucesso", "Matrícula excluída!", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro",
                $"Erro ao excluir matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AtualizarLista(IEnumerable<MatriculaDto> resultados)
    {
        Matriculas.Clear();

        foreach (var matricula in resultados)
            Matriculas.Add(matricula);
    }
}