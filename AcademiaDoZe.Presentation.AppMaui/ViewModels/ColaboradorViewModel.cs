
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(ColaboradorId), "Id")]
public partial class ColaboradorViewModel : BaseViewModel
{
    private static string T(string key) =>
        new System.Resources.ResourceManager(
            "AcademiaDoZe.Presentation.AppMaui.Resources.Strings.AppResources",
            typeof(ColaboradorViewModel).Assembly)
        .GetString(key, System.Globalization.CultureInfo.CurrentUICulture) ?? key;


    public IEnumerable<AppColaboradorTipo> ColaboradorTipos { get; } =
        Enum.GetValues<AppColaboradorTipo>();

    public IEnumerable<AppColaboradorVinculo> ColaboradorVinculos { get; } =
        Enum.GetValues<AppColaboradorVinculo>();

    private readonly IColaboradorService _colaboradorService;
    private readonly ILogradouroService _logradouroService;

    private ColaboradorDto _colaborador = CriarColaboradorPadrao();

    public ColaboradorDto Colaborador
    {
        get => _colaborador;
        set => SetProperty(ref _colaborador, value);
    }

    private int _colaboradorId;

    public int ColaboradorId
    {
        get => _colaboradorId;
        set => SetProperty(ref _colaboradorId, value);
    }

    private bool _isEditMode;

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private string _confirmarSenha = string.Empty;

    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }

    public bool HasEnderecoVinculado =>
        Colaborador?.Endereco != null && Colaborador.Endereco.Id > 0;

    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;

    public ColaboradorViewModel(
        IColaboradorService colaboradorService,
        ILogradouroService logradouroService)
    {
        _colaboradorService = colaboradorService;
        _logradouroService = logradouroService;
        Title = T("vmMsg037");
    }

    public async Task InitializeAsync()
    {
        ConfirmarSenha = string.Empty;

        if (ColaboradorId > 0)
        {
            IsEditMode = true;
            Title = T("vmMsg038");
            await LoadColaboradorAsync();
        }
        else
        {
            IsEditMode = false;
            Title = T("vmMsg039");
            Colaborador = CriarColaboradorPadrao();
            NotificarStatusEndereco();
        }
    }

    private void NotificarStatusEndereco()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }

    private static ColaboradorDto CriarColaboradorPadrao() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Senha = string.Empty,
        Endereco = new LogradouroDto
        {
            Cep = string.Empty,
            Nome = string.Empty,
            Bairro = string.Empty,
            Cidade = string.Empty,
            Estado = string.Empty,
            Pais = "Brasil"
        },
        Numero = string.Empty,
        Complemento = string.Empty,
        DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
        Tipo = AppColaboradorTipo.Atendente,
        Vinculo = AppColaboradorVinculo.CLT
    };

    
    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task LoadColaboradorAsync()
    {
        if (ColaboradorId <= 0)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var colaboradorData = await _colaboradorService.ObterPorIdAsync(ColaboradorId, cts.Token);

            if (colaboradorData != null)
            {
                colaboradorData.Endereco ??= new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                };

                colaboradorData.Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                Colaborador = colaboradorData;
                NotificarStatusEndereco();
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg040"),
                T("vmMsg041"), "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg005"),
                T("vmMsg042").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchByCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(Colaborador.Cpf))
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg003"),
                T("vmMsg043"), "OK");
            return;
        }

        var apenasDigitos = new string(Colaborador.Cpf.Where(char.IsDigit).ToArray());

        if (apenasDigitos.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg007"),
                T("vmMsg044"), "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var colaboradorData = await _colaboradorService.ObterPorCpfAsync(apenasDigitos, cts.Token);

            if (colaboradorData != null)
            {
                colaboradorData.Endereco ??= new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                };

                colaboradorData.Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                Colaborador = colaboradorData;
                ColaboradorId = colaboradorData.Id;
                IsEditMode = true;
                Title = T("vmMsg038");
                NotificarStatusEndereco();

                await Shell.Current.DisplayAlertAsync(T("vmMsg003"),
                    T("vmMsg045"), "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(T("vmMsg003"), T("vmMsg009"), "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg040"),
                T("vmMsg046"), "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg005"),
                T("vmMsg011").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchByCepAsync()
    {
        if (Colaborador.Endereco == null ||
            string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg003"),
                T("vmMsg047"), "OK");
            return;
        }

        var apenasDigitos = new string(Colaborador.Endereco.Cep.Where(char.IsDigit).ToArray());

        if (apenasDigitos.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg007"),
                T("vmMsg048"), "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var logradouroData = await _logradouroService.ObterPorCepAsync(apenasDigitos, cts.Token);

            if (logradouroData != null)
            {
                Colaborador.Endereco = logradouroData;
                OnPropertyChanged(nameof(Colaborador));
                NotificarStatusEndereco();

                await Shell.Current.DisplayAlertAsync(T("vmMsg003"),
                    T("vmMsg049"), "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(T("vmMsg003"),
                    T("vmMsg050"), "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg040"),
                T("vmMsg051"), "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg005"),
                T("vmMsg014").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveColaboradorAsync()
    {
        if (IsBusy)
            return;

        if (!await ValidateColaboradorAsync(Colaborador, IsEditMode, ConfirmarSenha))
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (Colaborador.Endereco != null &&
                Colaborador.Endereco.Id <= 0 &&
                !string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
            {
                var cepLimpo = new string(Colaborador.Endereco.Cep.Where(char.IsDigit).ToArray());

                if (cepLimpo.Length == 8)
                {
                    var logradouroData = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);

                    if (logradouroData != null)
                    {
                        Colaborador.Endereco = logradouroData;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlertAsync(T("vmMsg052"),
                            T("vmMsg053"), "OK");
                        return;
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(T("vmMsg007"),
                        T("vmMsg054"), "OK");
                    return;
                }
            }

            Colaborador.Nome = Colaborador.Nome.Trim();
            Colaborador.Cpf = new string(Colaborador.Cpf.Where(char.IsDigit).ToArray());
            Colaborador.Telefone = new string(Colaborador.Telefone.Where(char.IsDigit).ToArray());
            Colaborador.Numero = Colaborador.Numero.Trim();
            Colaborador.Complemento = string.IsNullOrWhiteSpace(Colaborador.Complemento)
                ? null : Colaborador.Complemento.Trim();
            Colaborador.Email = string.IsNullOrWhiteSpace(Colaborador.Email)
                ? null : Colaborador.Email.Trim();
            Colaborador.Senha = string.IsNullOrWhiteSpace(Colaborador.Senha)
                ? null : Colaborador.Senha.Trim();

            if (IsEditMode)
            {
                await _colaboradorService.AtualizarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync(T("vmMsg033"),
                    T("vmMsg055"), "OK");
            }
            else
            {
                await _colaboradorService.AdicionarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync(T("vmMsg033"),
                    T("vmMsg056"), "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg040"),
                T("vmMsg057"), "OK");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg058"), ex.Message, "OK");
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg007"), ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg005"),
                T("vmMsg059").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SelecionarFotoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheetAsync(
                T("vmMsg015"), T("vmMsg016"), T("vmMsg017"), T("vmMsg018"), T("vmMsg019"));

            if (escolha == T("vmMsg017"))
            {
                Colaborador.Foto = null;
                OnPropertyChanged(nameof(Colaborador));
                return;
            }

            FileResult? result = null;

            if (escolha == T("vmMsg018"))
            {
                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = T("vmMsg020"),
                    FileTypes = FilePickerFileType.Images
                });
            }
            else if (escolha == T("vmMsg019"))
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    result = await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(T("vmMsg005"),
                        T("vmMsg060"), "OK");
                    return;
                }
            }

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                Colaborador.Foto = new ArquivoDto { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Colaborador));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(T("vmMsg005"),
                T("vmMsg061").Replace("{ex.Message}", ex.Message), "OK");
        }
    }

    private static async Task<bool> ValidateColaboradorAsync(
        ColaboradorDto colaborador, bool isEditMode, string confirmarSenha)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(colaborador.Nome))
            errors.Add(T("vmMsg062"));

        if (string.IsNullOrWhiteSpace(colaborador.Cpf))
        {
            errors.Add(T("vmMsg063"));
        }
        else if (new string(colaborador.Cpf.Where(char.IsDigit).ToArray()).Length != 11)
        {
            errors.Add(T("vmMsg064"));
        }

        if (colaborador.DataNascimento == default)
        {
            errors.Add(T("vmMsg065"));
        }
        else
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            if (colaborador.DataNascimento > hoje)
                errors.Add(T("vmMsg066"));
            else if (colaborador.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12)))
                errors.Add(T("vmMsg067"));
        }

        if (string.IsNullOrWhiteSpace(colaborador.Telefone))
        {
            errors.Add(T("vmMsg068"));
        }
        else if (new string(colaborador.Telefone.Where(char.IsDigit).ToArray()).Length != 11)
        {
            errors.Add(T("vmMsg069"));
        }

        if (!string.IsNullOrWhiteSpace(colaborador.Email) &&
            !ValidarFormatoEmail(colaborador.Email.Trim()))
        {
            errors.Add(T("vmMsg070"));
        }

        if (!isEditMode)
        {
            if (string.IsNullOrWhiteSpace(colaborador.Senha))
            {
                errors.Add(T("vmMsg071"));
            }
            else
            {
                var senha = colaborador.Senha.Trim();

                if (senha.Length < 6 || !senha.Any(char.IsUpper))
                    errors.Add(T("vmMsg072"));

                if (colaborador.Senha != confirmarSenha)
                    errors.Add(T("vmMsg073"));
            }
        }
        else if (!string.IsNullOrWhiteSpace(colaborador.Senha))
        {
            var senha = colaborador.Senha.Trim();

            if (senha.Length < 6 || !senha.Any(char.IsUpper))
                errors.Add(T("vmMsg074"));

            if (colaborador.Senha != confirmarSenha)
                errors.Add(T("vmMsg073"));
        }

        if (colaborador.Endereco == null ||
            string.IsNullOrWhiteSpace(colaborador.Endereco.Cep))
        {
            errors.Add(T("vmMsg075"));
        }

        if (string.IsNullOrWhiteSpace(colaborador.Numero))
            errors.Add(T("vmMsg076"));

        if (colaborador.DataAdmissao == default)
            errors.Add(T("vmMsg077"));
        else if (colaborador.DataAdmissao > DateOnly.FromDateTime(DateTime.Today))
            errors.Add(T("vmMsg078"));

        if (colaborador.Tipo == AppColaboradorTipo.Administrador &&
            colaborador.Vinculo != AppColaboradorVinculo.CLT)
        {
            errors.Add(T("vmMsg079"));
        }

        if (errors.Count > 0)
        {
            var mensagem = T("vmMsg080") +
                           string.Join("\n", errors);

            await Shell.Current.DisplayAlertAsync(T("vmMsg081"), mensagem, "OK");
            return false;
        }

        return true;
    }

    private static bool ValidarFormatoEmail(string email)
    {
        var partes = email.Split('@');
        if (partes.Length != 2) return false;
        if (string.IsNullOrWhiteSpace(partes[0])) return false;

        var dominio = partes[1];
        if (string.IsNullOrWhiteSpace(dominio)) return false;
        if (dominio.StartsWith('.') || dominio.EndsWith('.')) return false;

        var labels = dominio.Split('.');
        if (labels.Length < 2) return false;
        if (labels.Any(string.IsNullOrWhiteSpace)) return false;

        return true;
    }
}

