using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
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
        Title = "Detalhes do Aluno";
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
            Title = "Editar Aluno";
            await LoadAlunoAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Aluno";
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
                    "Aviso", "Aluno não encontrado.", "OK");
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
                "Erro", $"Erro ao carregar aluno: {ex.Message}", "OK");
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
                "Validação", "Informe os 11 dígitos do CPF.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var encontrado = await _alunoService.ObterPorCpfAsync(cpf);

            if (encontrado == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Aviso", "CPF não encontrado.", "OK");
                return;
            }

            encontrado.Endereco ??= CriarEnderecoPadrao();
            encontrado.Senha = string.Empty;

            Aluno = encontrado;
            AlunoId = encontrado.Id;
            IsEditMode = true;
            Title = "Editar Aluno";
            ConfirmarSenha = string.Empty;
            NotificarEndereco();

            await Shell.Current.DisplayAlertAsync(
                "Aviso", "Aluno encontrado! Dados carregados para edição.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
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
                "Validação", "Informe os 8 dígitos do CEP.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var endereco = await _logradouroService.ObterPorCepAsync(cep);

            if (endereco == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Aviso", "CEP não cadastrado. Cadastre o logradouro primeiro.", "OK");
                return;
            }

            Aluno.Endereco = endereco;
            OnPropertyChanged(nameof(Aluno));
            NotificarEndereco();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro", $"Erro ao buscar CEP: {ex.Message}", "OK");
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
                "Origem da Imagem", "Cancelar", "Remover Foto",
                "Galeria", "Câmera");

            if (escolha == "Remover Foto")
            {
                Aluno.Foto = null;
                OnPropertyChanged(nameof(Aluno));
                return;
            }

            FileResult? resultado = null;

            if (escolha == "Galeria")
            {
                resultado = await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = "Selecione uma imagem",
                        FileTypes = FilePickerFileType.Images
                    });
            }
            else if (escolha == "Câmera")
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Aviso", "Câmera indisponível neste dispositivo.", "OK");
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
                "Erro", $"Erro ao selecionar foto: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveAlunoAsync()
    {
        if (IsBusy) return;

        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Aluno.Nome))
            erros.Add("Nome é obrigatório.");

        var cpf = new string((Aluno.Cpf ?? string.Empty)
            .Where(char.IsDigit).ToArray());

        if (cpf.Length != 11)
            erros.Add("CPF deve conter 11 dígitos.");

        var telefone = new string((Aluno.Telefone ?? string.Empty)
            .Where(char.IsDigit).ToArray());

        if (telefone.Length != 11)
            erros.Add("Telefone deve conter 11 dígitos com DDD.");

        if (Aluno.DataNascimento == default ||
            Aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today))
            erros.Add("Informe uma data de nascimento válida.");

        if (Aluno.Endereco == null ||
            string.IsNullOrWhiteSpace(Aluno.Endereco.Cep))
            erros.Add("Endereço é obrigatório.");

        if (string.IsNullOrWhiteSpace(Aluno.Numero))
            erros.Add("Número do endereço é obrigatório.");

        if (!IsEditMode && string.IsNullOrWhiteSpace(Aluno.Senha))
            erros.Add("Senha é obrigatória no cadastro.");

        if (!string.IsNullOrWhiteSpace(Aluno.Senha) &&
            Aluno.Senha != ConfirmarSenha)
            erros.Add("A confirmação de senha não confere.");

        if (erros.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync(
                "Validação", string.Join("\n", erros), "OK");
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
                        "Endereço", "Cadastre o logradouro antes de salvar o aluno.", "OK");
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
                    "Sucesso", "Aluno atualizado com sucesso!", "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno);
                await Shell.Current.DisplayAlertAsync(
                    "Sucesso", "Aluno cadastrado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro", $"Erro ao salvar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
