using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using AcademiaDoZe.Presentation.AppMaui.Helpers;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(LogradouroId), "Id")]
public partial class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    private LogradouroDto _logradouro = new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = string.Empty
    };

    public LogradouroDto Logradouro
    {
        get => _logradouro;
        set => SetProperty(ref _logradouro, value);
    }

    private int _logradouroId;

    public int LogradouroId
    {
        get => _logradouroId;
        set => SetProperty(ref _logradouroId, value);
    }

    private bool _isEditMode;

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public LogradouroViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = LocalizationManager.Instance["strCPFDoAluno"];
    }

    public async Task InitializeAsync()
    {
        if (LogradouroId > 0)
        {
            IsEditMode = true;
            Title = LocalizationManager.Instance["strBusqueUmAlunoCadastradoPeloCPF"];
            await LoadLogradouroAsync();
        }
        else
        {
            IsEditMode = false;
            Title = LocalizationManager.Instance["strPlanoEVigencia"];

            Logradouro = new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = "Brasil"
            };
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task LoadLogradouroAsync()
    {
        if (LogradouroId <= 0)
            return;

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var logradouroData =
                await _logradouroService.ObterPorIdAsync(
                    LogradouroId,
                    cts.Token);

            if (logradouroData != null)
            {
                Logradouro = logradouroData;
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg017"],
                LocalizationManager.Instance["strTipoDePlano"],
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"],
                string.Format(LocalizationManager.Instance["strErroAoCarregarLogradouro0"], ex.Message),
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        if (string.IsNullOrWhiteSpace(Logradouro.Cep))
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["vmMsg003"],
                LocalizationManager.Instance["vmMsg047"],
                "OK");

            return;
        }

        var apenasDigitos =
            new string([.. Logradouro.Cep.Where(char.IsDigit)]);

        if (apenasDigitos.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg015"],
                LocalizationManager.Instance["vmMsg048"],
                "OK");

            return;
        }

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var logradouroData =
                await _logradouroService.ObterPorCepAsync(
                    apenasDigitos,
                    cts.Token);

            if (logradouroData != null)
            {
                Logradouro = logradouroData;
                LogradouroId = logradouroData.Id;
                IsEditMode = true;
                Title = LocalizationManager.Instance["strBusqueUmAlunoCadastradoPeloCPF"];

                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["vmMsg003"],
                    LocalizationManager.Instance["strDataDeInicio"],
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["vmMsg003"],
                    LocalizationManager.Instance["strDataDeTerminoAutomatica"],
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg017"],
                LocalizationManager.Instance["strObjetivoDaMatricula"],
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"],
                string.Format(LocalizationManager.Instance["strErroAoBuscarCEP0"], ex.Message),
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveLogradouroAsync()
    {
        if (IsBusy)
            return;

        if (!await ValidateLogradouroAsync(Logradouro))
            return;

        try
        {
            IsBusy = true;

            using var cts =
                new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Logradouro.Cep =
                new string([.. Logradouro.Cep.Where(char.IsDigit)]);

            Logradouro.Estado =
                Logradouro.Estado.Trim().ToUpperInvariant();

            Logradouro.Nome = Logradouro.Nome.Trim();
            Logradouro.Bairro = Logradouro.Bairro.Trim();
            Logradouro.Cidade = Logradouro.Cidade.Trim();

            Logradouro.Pais =
                string.IsNullOrWhiteSpace(Logradouro.Pais)
                    ? "Brasil"
                    : Logradouro.Pais.Trim();

            if (IsEditMode)
            {
                await _logradouroService.AtualizarAsync(
                    Logradouro,
                    cts.Token);

                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["listMsg008"],
                    LocalizationManager.Instance["strExHipertrofiaCondicionamento"],
                    "OK");
            }
            else
            {
                await _logradouroService.AdicionarAsync(
                    Logradouro,
                    cts.Token);

                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["listMsg008"],
                    LocalizationManager.Instance["strRestricoesMedicas"],
                    "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg017"],
                LocalizationManager.Instance["strSelecioneTodasAsRestricoesAplicaveis"],
                "OK");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["vmMsg058"],
                ex.Message,
                "OK");
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg015"],
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"],
                string.Format(LocalizationManager.Instance["strErroAoSalvarLogradouro0"], ex.Message),
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<bool> ValidateLogradouroAsync(
        LogradouroDto logradouro)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(logradouro.Cep))
        {
            errors.Add(LocalizationManager.Instance["strDiabetes"]);
        }
        else
        {
            var cepDigitos =
                new string([.. logradouro.Cep.Where(char.IsDigit)]);

            if (cepDigitos.Length != 8)
            {
                errors.Add(
                    LocalizationManager.Instance["strPressaoAlta"]);
            }
        }

        if (string.IsNullOrWhiteSpace(logradouro.Nome))
        {
            errors.Add(LocalizationManager.Instance["strLabirintite"]);
        }

        if (string.IsNullOrWhiteSpace(logradouro.Bairro))
        {
            errors.Add(LocalizationManager.Instance["strAlergias"]);
        }

        if (string.IsNullOrWhiteSpace(logradouro.Cidade))
        {
            errors.Add(LocalizationManager.Instance["strProblemasRespiratorios"]);
        }

        if (string.IsNullOrWhiteSpace(logradouro.Estado))
        {
            errors.Add(LocalizationManager.Instance["strRemedioContinuo"]);
        }
        else
        {
            var estadoLimpo = logradouro.Estado.Trim();

            if (estadoLimpo.Length != 2 ||
                !estadoLimpo.All(char.IsLetter))
            {
                errors.Add(
                    LocalizationManager.Instance["strObservacoes"]);
            }
        }

        if (string.IsNullOrWhiteSpace(logradouro.Pais))
        {
            errors.Add(LocalizationManager.Instance["strDescrevaAsRestricoesMedicas"]);
        }

        if (errors.Count > 0)
        {
            var mensagem =
                LocalizationManager.Instance["strLaudoMedicoPDF"] +
                string.Join("\n", errors);

            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["vmMsg081"],
                mensagem,
                "OK");

            return false;
        }

        return true;
    }
}