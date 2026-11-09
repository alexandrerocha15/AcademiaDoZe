using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    public ObservableCollection<string> FilterTypes { get; } =
        ["Nome", "CPF", "Id", "Email"];

    private ObservableCollection<AlunoDto> _alunos = [];
    public ObservableCollection<AlunoDto> Alunos
    {
        get => _alunos;
        set => SetProperty(ref _alunos, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Alunos";
    }

    [RelayCommand]
    private async Task AddAlunoAsync()
    {
        await Shell.Current.GoToAsync("aluno");
    }

    [RelayCommand]
    private async Task EditAlunoAsync(AlunoDto aluno)
    {
        if (aluno == null) return;

        await Shell.Current.GoToAsync($"aluno?Id={aluno.Id}");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadAlunosAsync();
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Alunos.Clear();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunos = await _alunoService.ObterTodosAsync(cts.Token);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var aluno in alunos)
                    Alunos.Add(aluno);
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro", $"Erro ao carregar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchAlunosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Alunos.Clear();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            IEnumerable<AlunoDto> resultados = [];

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _alunoService.ObterTodosAsync(cts.Token);
            }
            else if (SelectedFilterType == "Nome")
            {
                resultados = await _alunoService.ObterPorNomeAsync(
                    SearchText.Trim(), cts.Token);
            }
            else if (SelectedFilterType == "CPF")
            {
                var cpf = new string(SearchText.Where(char.IsDigit).ToArray());

                var todos = await _alunoService.ObterTodosAsync(cts.Token);

                resultados = todos.Where(a =>
                    new string((a.Cpf ?? string.Empty).Where(char.IsDigit).ToArray())
                        .Contains(cpf));
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(SearchText.Trim(), out int id) || id <= 0)
                {
                    return;
                }

                var aluno = await _alunoService.ObterPorIdAsync(id, cts.Token);
                if (aluno != null) resultados = [aluno];
            }
            else if (SelectedFilterType == "Email")
            {
                var aluno = await _alunoService.ObterPorEmailAsync(
                    SearchText.Trim(), cts.Token);

                if (aluno != null) resultados = [aluno];
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var aluno in resultados)
                    Alunos.Add(aluno);
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro", $"Erro ao buscar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAlunoAsync(AlunoDto aluno)
    {
        if (aluno == null) return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o aluno {aluno.Nome}?",
            "Sim", "Não");

        if (!confirmar) return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool sucesso = await _alunoService.RemoverAsync(aluno.Id, cts.Token);

            if (sucesso)
            {
                Alunos.Remove(aluno);
                await Shell.Current.DisplayAlertAsync(
                    "Sucesso", "Aluno excluído!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Erro", "Não foi possível excluir o aluno.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro", $"Erro ao excluir aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
