using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private static string T(string key) =>
        new System.Resources.ResourceManager(
            "AcademiaDoZe.Presentation.AppMaui.Resources.Strings.AppResources",
            typeof(AlunoViewModel).Assembly)
        .GetString(key, System.Globalization.CultureInfo.CurrentUICulture) ?? key;


    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;

    private AlunoDto _aluno = CriarAlunoPadrao();

    public AlunoDto Aluno
    {
        get => _aluno;
        set => SetProperty(ref _aluno, value);
    }

    private int _alunoId;
    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
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
        Aluno.Endereco != null && Aluno.Endereco.Id > 0;

    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;

    public AlunoViewModel(
        IAlunoService alunoService,
        ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        Title = T("vmMsg000");
    }

    private static AlunoDto CriarAlunoPadrao() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Senha = string.Empty,
        Endereco = CriarEnderecoPadrao(),
        Numero = string.Empty,
        Complemento = string.Empty
    };

    private static LogradouroDto CriarEnderecoPadrao() => new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };

    private void NotificarEndereco()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }

    public async Task InitializeAsync()
    {
        ConfirmarSenha = string.Empty;

        if (AlunoId > 0)
        {
            IsEditMode = true;
            Title = T("vmMsg001");
            await LoadAlunoAsync();
        }
        else
        {
            IsEditMode = false;
            Title = T("vmMsg002");
            Aluno = CriarAlunoPadrao();
            NotificarEndereco();
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task LoadAlunoAsync()
    {
        if (AlunoId <= 0) return;

        try
        {
            IsBusy = true;
            var aluno = await _alunoService.ObterPorIdAsync(AlunoId);

            if (aluno == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    T("vmMsg003"), T("vmMsg004"), "OK");
                return;
            }

            aluno.Endereco ??= CriarEnderecoPadrao();
            aluno.Senha = string.Empty;
            ConfirmarSenha = string.Empty;
            Aluno = aluno;
            NotificarEndereco();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                T("vmMsg005"), T("vmMsg006").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        var cpf = new string((Aluno.Cpf ?? string.Empty)
            .Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync(
                T("vmMsg007"), T("vmMsg008"), "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var encontrado = await _alunoService.ObterPorCpfAsync(cpf);

            if (encontrado == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    T("vmMsg003"), T("vmMsg009"), "OK");
                return;
            }

            encontrado.Endereco ??= CriarEnderecoPadrao();
            encontrado.Senha = string.Empty;

            Aluno = encontrado;
            AlunoId = encontrado.Id;
            IsEditMode = true;
            Title = T("vmMsg001");
            ConfirmarSenha = string.Empty;
            NotificarEndereco();

            await Shell.Current.DisplayAlertAsync(
                T("vmMsg003"), T("vmMsg010"), "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                T("vmMsg005"), T("vmMsg011").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        var cep = new string((Aluno.Endereco?.Cep ?? string.Empty)
            .Where(char.IsDigit).ToArray());

        if (cep.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync(
                T("vmMsg007"), T("vmMsg012"), "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var endereco = await _logradouroService.ObterPorCepAsync(cep);

            if (endereco == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    T("vmMsg003"), T("vmMsg013"), "OK");
                return;
            }

            Aluno.Endereco = endereco;
            OnPropertyChanged(nameof(Aluno));
            NotificarEndereco();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                T("vmMsg005"), T("vmMsg014").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelecionarFotoAsync()
    {
        try
        {
            var escolha = await Shell.Current.DisplayActionSheetAsync(
                T("vmMsg015"), T("vmMsg016"), T("vmMsg017"),
                T("vmMsg018"), T("vmMsg019"));

            if (escolha == T("vmMsg017"))
            {
                Aluno.Foto = null;
                OnPropertyChanged(nameof(Aluno));
                return;
            }

            FileResult? resultado = null;

            if (escolha == T("vmMsg018"))
            {
                resultado = await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = T("vmMsg020"),
                        FileTypes = FilePickerFileType.Images
                    });
            }
            else if (escolha == T("vmMsg019"))
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlertAsync(
                        T("vmMsg003"), T("vmMsg021"), "OK");
                    return;
                }

                resultado = await MediaPicker.Default.CapturePhotoAsync();
            }

            if (resultado == null) return;

            using var stream = await resultado.OpenReadAsync();
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria);

            Aluno.Foto = new ArquivoDto
            {
                Conteudo = memoria.ToArray()
            };

            OnPropertyChanged(nameof(Aluno));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                T("vmMsg005"), T("vmMsg022").Replace("{ex.Message}", ex.Message), "OK");
        }
    }

    [RelayCommand]
    private async Task SaveAlunoAsync()
    {
        if (IsBusy) return;

        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Aluno.Nome))
            erros.Add(T("vmMsg023"));

        var cpf = new string((Aluno.Cpf ?? string.Empty)
            .Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            erros.Add(T("vmMsg024"));

        var telefone = new string((Aluno.Telefone ?? string.Empty)
            .Where(char.IsDigit).ToArray());

        if (telefone.Length != 11)
            erros.Add(T("vmMsg025"));

        if (Aluno.DataNascimento == default ||
            Aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today))
            erros.Add(T("vmMsg026"));

        if (Aluno.Endereco == null ||
            string.IsNullOrWhiteSpace(Aluno.Endereco.Cep))
            erros.Add(T("vmMsg027"));

        if (string.IsNullOrWhiteSpace(Aluno.Numero))
            erros.Add(T("vmMsg028"));

        if (!IsEditMode && string.IsNullOrWhiteSpace(Aluno.Senha))
            erros.Add(T("vmMsg029"));

        if (!string.IsNullOrWhiteSpace(Aluno.Senha) &&
            Aluno.Senha != ConfirmarSenha)
            erros.Add(T("vmMsg030"));

        if (erros.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync(
                T("vmMsg007"), string.Join("\n", erros), "OK");
            return;
        }

        try
        {
            IsBusy = true;

            var cep = new string(Aluno.Endereco!.Cep
                .Where(char.IsDigit).ToArray());

            if (Aluno.Endereco.Id <= 0)
            {
                var endereco = await _logradouroService.ObterPorCepAsync(cep);

                if (endereco == null)
                {
                    await Shell.Current.DisplayAlertAsync(
                        T("vmMsg031"), T("vmMsg032"), "OK");
                    return;
                }

                Aluno.Endereco = endereco;
            }

            Aluno.Nome = Aluno.Nome.Trim();
            Aluno.Cpf = cpf;
            Aluno.Telefone = telefone;
            Aluno.Numero = Aluno.Numero.Trim();
            Aluno.Email = string.IsNullOrWhiteSpace(Aluno.Email)
                ? null : Aluno.Email.Trim();
            Aluno.Senha = string.IsNullOrWhiteSpace(Aluno.Senha)
                ? null : Aluno.Senha;

            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(Aluno);
                await Shell.Current.DisplayAlertAsync(
                    T("vmMsg033"), T("vmMsg034"), "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno);
                await Shell.Current.DisplayAlertAsync(
                    T("vmMsg033"), T("vmMsg035"), "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                T("vmMsg005"), T("vmMsg036").Replace("{ex.Message}", ex.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
