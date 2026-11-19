using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Helpers;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoColaboradorViewModel : BaseViewModel
{
    private readonly IColaboradorService _pessoas;
    private readonly IAcessoColaboradorService _acessos;
    public ObservableCollection<ColaboradorDto> Pessoas { get; } = [];

    private ColaboradorDto? _selecionado;
    public ColaboradorDto? Selecionado
    {
        get => _selecionado;
        set => SetProperty(ref _selecionado, value);
    }

    private string _resultado = string.Empty;
    public string Resultado
    {
        get => _resultado;
        set => SetProperty(ref _resultado, value);
    }

    public AcessoColaboradorViewModel(IColaboradorService pessoas, IAcessoColaboradorService acessos)
    {
        _pessoas = pessoas;
        _acessos = acessos;
        Title = LocalizationManager.Instance["strAcessoColaboradorTitulo"];
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Pessoas.Clear();
            foreach (var pessoa in await _pessoas.ObterTodosAsync())
                Pessoas.Add(pessoa);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(LocalizationManager.Instance["strAcessoErro"], ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RegistrarAsync()
    {
        if (IsBusy) return;
        if (Selecionado is null)
        {
            await Shell.Current.DisplayAlertAsync(LocalizationManager.Instance["strAcessoAviso"],
                LocalizationManager.Instance["strAcessoSelecione"], "OK");
            return;
        }
        try
        {
            IsBusy = true;
            var acesso = await _acessos.RegistrarAsync(Selecionado.Id);
            var chave = acesso.Entrada ? "strAcessoEntrada" : "strAcessoSaida";
            var descricao = acesso.Entrada
                ? "strAcessoEntradaRegistrada"
                : "strAcessoTotalDia";
            Resultado = $"{LocalizationManager.Instance[chave]} — {Selecionado.Nome} — {acesso.DataHora:g}";
            if (false || !acesso.Entrada)
                Resultado += $"\n{LocalizationManager.Instance[descricao]}: {(int)acesso.Duracao.TotalHours}h {acesso.Duracao.Minutes}min";
            await Shell.Current.DisplayAlertAsync(LocalizationManager.Instance["strAcessoAviso"], Resultado, "OK");
        }
        catch (Exception ex)
        {
            Resultado = ex.Message;
            await Shell.Current.DisplayAlertAsync(LocalizationManager.Instance["strAcessoErro"], ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }
}
