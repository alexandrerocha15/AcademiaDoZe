using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using AcademiaDoZe.Presentation.AppMaui.Helpers;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;

    public IEnumerable<AppMatriculaPlano> PlanoTipos { get; } =
        Enum.GetValues<AppMatriculaPlano>();

    public IEnumerable<AppMatriculaRestricoes> RestricoesTipos { get; } =
    Enum.GetValues<AppMatriculaRestricoes>()
        .Where(r => r != AppMatriculaRestricoes.None);

    private string _nomeLaudo = LocalizationManager.Instance["strExcluir003"];

    public string NomeLaudo
    {
        get => _nomeLaudo;
        set => SetProperty(ref _nomeLaudo, value);
    }

    public bool TemLaudo => Matricula.LaudoMedico is not null;

    public bool TemRestricoes =>
        Matricula.RestricoesMedicas != AppMatriculaRestricoes.None;

    public bool RestricaoSelecionada(AppMatriculaRestricoes restricao) =>
        Matricula.RestricoesMedicas.HasFlag(restricao);

    public void AtualizarRestricao(
        AppMatriculaRestricoes restricao, bool selecionada)
    {
        if (selecionada)
            Matricula.RestricoesMedicas |= restricao;
        else
            Matricula.RestricoesMedicas &= ~restricao;

        OnPropertyChanged(nameof(TemRestricoes));
        OnPropertyChanged(nameof(Matricula));
    }

    private MatriculaDto _matricula = CriarMatriculaPadrao();

    public MatriculaDto Matricula
    {
        get => _matricula;
        set
        {
            if (SetProperty(ref _matricula, value))
                NotificarAluno();
        }
    }

    private int _matriculaId;
    public int MatriculaId
    {
        get => _matriculaId;
        set => SetProperty(ref _matriculaId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public bool HasAlunoVinculado =>
        Matricula.AlunoMatricula?.Id > 0;

    public bool HasNoAlunoVinculado => !HasAlunoVinculado;

    public MatriculaViewModel(
        IMatriculaService matriculaService,
        IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        Title = LocalizationManager.Instance["strCancelar004"];
    }

    private static MatriculaDto CriarMatriculaPadrao() => new()
    {
        AlunoMatricula = new AlunoDto
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(
                DateTime.Today.AddYears(-18)),
            Telefone = string.Empty,
            Endereco = new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = "Brasil"
            },
            Numero = string.Empty
        },
        Plano = AppMatriculaPlano.Mensal,
        DataInicio = DateOnly.FromDateTime(DateTime.Today),
        DataFim = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
        Objetivo = string.Empty,
        RestricoesMedicas = AppMatriculaRestricoes.None,
        ObservacoesRestricoes = string.Empty
    };

    private void NotificarAluno()
    {
        OnPropertyChanged(nameof(HasAlunoVinculado));
        OnPropertyChanged(nameof(HasNoAlunoVinculado));
    }

    public async Task InitializeAsync()
    {
        if (MatriculaId > 0)
        {
            IsEditMode = true;
            Title = LocalizationManager.Instance["strSalvar005"];
            await LoadMatriculaAsync();
        }
        else
        {
            IsEditMode = false;
            Title = LocalizationManager.Instance["strRemover"];
            Matricula = CriarMatriculaPadrao();
        }

        NomeLaudo = Matricula.LaudoMedico is null
            ? LocalizationManager.Instance["strExcluir003"]
            : LocalizationManager.Instance["strNomeRua"];

        OnPropertyChanged(nameof(TemLaudo));
        OnPropertyChanged(nameof(TemRestricoes));

        NotificarAluno();
    }

    [RelayCommand]
    private async Task LoadMatriculaAsync()
    {
        if (MatriculaId <= 0)
            return;

        try
        {
            IsBusy = true;
            var resultado = await _matriculaService
                .ObterPorIdAsync(MatriculaId);

            if (resultado is not null)
                Matricula = resultado;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"], string.Format(LocalizationManager.Instance["strErroAoCarregarMatricula0"], ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        var cpf = new string(
            (Matricula.AlunoMatricula?.Cpf ?? string.Empty)
                .Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg015"], LocalizationManager.Instance["vmMsg008"], "OK");
            return;
        }

        try
        {
            IsBusy = true;

            var aluno = await _alunoService.ObterPorCpfAsync(cpf);

            if (aluno is null)
            {
                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["vmMsg003"], LocalizationManager.Instance["vmMsg004"], "OK");
                return;
            }

            Matricula.AlunoMatricula = aluno;
            OnPropertyChanged(nameof(Matricula));
            NotificarAluno();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"], string.Format(LocalizationManager.Instance["strErroAoBuscarAluno0"], ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveMatriculaAsync()
    {
        if (IsBusy)
            return;

        if (!HasAlunoVinculado)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg015"], LocalizationManager.Instance["strBairro"], "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Matricula.Objetivo))
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg015"], LocalizationManager.Instance["strCidade"], "OK");
            return;
        }

        if (Matricula.DataFim <= Matricula.DataInicio)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg015"], LocalizationManager.Instance["strEstado"], "OK");
            return;
        }

        if (TemRestricoes && string.IsNullOrWhiteSpace(Matricula.ObservacoesRestricoes))
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg015"],
                LocalizationManager.Instance["strPais"],
                "OK");
            return;
        }

        if (TemRestricoes && !TemLaudo)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg015"],
                LocalizationManager.Instance["strNome012"],
                "OK");
            return;
        }

        try
        {
            IsBusy = true;

            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(Matricula);

                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["listMsg008"], LocalizationManager.Instance["strBuscarCEP"], "OK");
            }
            else
            {
                if (await _matriculaService.PossuiMatriculaAtivaAsync(
                    Matricula.AlunoMatricula.Id))
                {
                    await Shell.Current.DisplayAlertAsync(
                        LocalizationManager.Instance["listMsg015"],
                        LocalizationManager.Instance["strDigiteOValor"],
                        "OK");
                    return;
                }

                await _matriculaService.AdicionarAsync(Matricula);

                await Shell.Current.DisplayAlertAsync(
                    LocalizationManager.Instance["listMsg008"], LocalizationManager.Instance["strNenhumLogradouroEncontrado"], "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"], string.Format(LocalizationManager.Instance["strErroAoSalvarMatricula0"], ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
    [RelayCommand]
    private async Task SelecionarLaudoAsync()
    {
        try
        {
            var arquivo = await FilePicker.Default.PickAsync(
                new PickOptions
                {
                    PickerTitle = LocalizationManager.Instance["strAdicionarPrimeiroLogradouro"],
                    FileTypes = FilePickerFileType.Pdf
                });

            if (arquivo is null)
                return;

            await using var stream = await arquivo.OpenReadAsync();
            using var memoria = new MemoryStream();

            await stream.CopyToAsync(memoria);

            Matricula.LaudoMedico = new ArquivoDto
            {
                Conteudo = memoria.ToArray()
            };

            NomeLaudo = arquivo.FileName;
            OnPropertyChanged(nameof(TemLaudo));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                LocalizationManager.Instance["listMsg001"], string.Format(LocalizationManager.Instance["strErroAoAnexarLaudo0"], ex.Message), "OK");
        }
    }

    [RelayCommand]
    private void RemoverLaudo()
    {
        Matricula.LaudoMedico = null;
        NomeLaudo = LocalizationManager.Instance["strExcluir003"];
        OnPropertyChanged(nameof(TemLaudo));
    }
}
