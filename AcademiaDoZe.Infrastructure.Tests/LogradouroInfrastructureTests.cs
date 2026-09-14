using AcademiaDoZe.Domain.Entities;
<<<<<<< HEAD
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
=======
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
>>>>>>> 91e6a53 (Adiciona Testes em Logradouro em Atv 05)

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _repository;

    public LogradouroInfrastructureTests()
    {
<<<<<<< HEAD
        _repository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    internal static async Task<Logradouro> CriarEInserirLogradouroAsync(
        LogradouroRepository logradouroRepo)
    {
        var cep = GerarCep();

        var logradouroResult = Logradouro.Criar(
            0, cep, "Rua de Teste", "Bairro Teste",
            "Lages", "SC", "Brasil");

        if (logradouroResult.IsFailure)
        {
            throw new Exception(
                $"Falha ao criar Logradouro: {string.Join(", ", logradouroResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await logradouroRepo.Adicionar(logradouroResult.Value!);
    }
=======
        DbInitializer
            .InicializarAsync(ConnectionString, DatabaseType)
            .GetAwaiter()
            .GetResult();

        _repository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    private static string GerarCep()
        => Random.Shared.Next(10000000, 99999999).ToString();
>>>>>>> 91e6a53 (Adiciona Testes em Logradouro em Atv 05)

    [Fact]
    public async Task Logradouro_Adicionar_E_ObterPorId_Sucesso()
    {
<<<<<<< HEAD
=======
        var logradouro = Logradouro.Criar(
            0, GerarCep(), "Alexandre", "Rocha",
            DatabaseType.ToString(), "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var resultado = await _repository.ObterPorId(logradouro.Id);

        Assert.NotNull(resultado);
        Assert.Equal(logradouro.Id, resultado.Id);
    }

    [Fact]
    public async Task Logradouro_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var resultado = await _repository.ObterPorId(999999);
        Assert.Null(resultado);
    }

    [Fact]
    public async Task Logradouro_ObterTodos_Sucesso()
    {
        var logradouro = Logradouro.Criar(
            0, GerarCep(), "Rua X", "Bairro Y",
            "CidadeUnica_" + Guid.NewGuid().ToString("N")[..5],
            "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var resultados = await _repository.ObterTodos();

        Assert.NotNull(resultados);
        Assert.Contains(resultados, x => x.Id == logradouro.Id);
    }

    [Fact]
    public async Task Logradouro_Atualizar_Sucesso()
    {
        var logradouro = Logradouro.Criar(
            0, GerarCep(), "Rua Antiga", "Bairro Antigo",
            "Lages", "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var atualizado = Logradouro.Criar(
            logradouro.Id, logradouro.Cep.ToString(),
            "Rua Nova", "Bairro Novo",
            "Florianópolis", "SC", "Brasil").Value!;

        await _repository.Atualizar(atualizado);

        var resultado = await _repository.ObterPorId(logradouro.Id);

        Assert.NotNull(resultado);
        Assert.Equal("Rua Nova", resultado.Nome);
        Assert.Equal("Bairro Novo", resultado.Bairro);
    }

    [Fact]
    public async Task Logradouro_Remover_Sucesso()
    {
        var logradouro = Logradouro.Criar(
            0, GerarCep(), "Rua Remover", "Centro",
            "Lages", "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var removido = await _repository.Remover(logradouro.Id);

        Assert.True(removido);
        Assert.Null(await _repository.ObterPorId(logradouro.Id));
    }

    [Fact]
    public async Task Logradouro_ObterPorCep_SucessoENulo()
    {
>>>>>>> 91e6a53 (Adiciona Testes em Logradouro em Atv 05)
        var cep = GerarCep();

        var logradouro = Logradouro.Criar(
            0, cep, "Rua das Flores", "Centro",
            "Lages", "SC", "Brasil").Value!;

<<<<<<< HEAD
        var inserido = await _repository.Adicionar(logradouro);

        Assert.NotNull(inserido);
        Assert.True(inserido.Id > 0);
        Assert.Equal(cep, inserido.Cep.Valor);
        Assert.Equal("Rua das Flores", inserido.Nome);

        var obtido = await _repository.ObterPorId(inserido.Id);

        Assert.NotNull(obtido);
        Assert.Equal(inserido.Id, obtido.Id);
        Assert.Equal(cep, obtido.Cep.Valor);
    }

    [Fact]
    public async Task Logradouro_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact]
    public async Task Logradouro_ObterTodos_Sucesso()
    {
        await CriarEInserirLogradouroAsync(_repository);

        var todos = await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Logradouro_Atualizar_Sucesso()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);
        var novoCep = GerarCep();

        var logradouroAtualizado = Logradouro.Criar(
            logradouro.Id, novoCep, "Rua Nova", "Bairro Novo",
            "Florianópolis", "SC", "Brasil").Value!;

        var resultado = await _repository.Atualizar(logradouroAtualizado);

        Assert.NotNull(resultado);
        Assert.Equal("Rua Nova", resultado.Nome);
        Assert.Equal("Bairro Novo", resultado.Bairro);
        Assert.Equal("Florianópolis", resultado.Cidade);

        var noBanco = await _repository.ObterPorId(logradouro.Id);

        Assert.NotNull(noBanco);
        Assert.Equal("Rua Nova", noBanco.Nome);
    }

    [Fact]
    public async Task Logradouro_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var cep = GerarCep();

        var logradouroInexistente = Logradouro.Criar(
            999999, cep, "Rua Fake", "Bairro Fake",
            "Cidade Fake", "SC", "Brasil").Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(
            () => _repository.Atualizar(logradouroInexistente));

        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Logradouro_Remover_Sucesso()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        var removido = await _repository.Remover(logradouro.Id);

        Assert.True(removido);

        var noBanco = await _repository.ObterPorId(logradouro.Id);

        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Logradouro_Remover_RetornaFalseQuandoInexistente()
    {
        var removida = await _repository.Remover(999999);
        Assert.False(removida);
    }

    [Fact]
    public async Task Logradouro_ObterPorCep_SucessoENulo()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        var obtido = await _repository.ObterPorCep(logradouro.Cep);

        Assert.NotNull(obtido);
        Assert.Equal(logradouro.Id, obtido.Id);

        var cepInexistente = Cep.Criar("99999999").Value!;
        var naoObtido = await _repository.ObterPorCep(cepInexistente);

        Assert.Null(naoObtido);
    }

    [Fact]
    public async Task Logradouro_CepJaExiste_ValidaçãoCorreta()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        var existe = await _repository.CepJaExiste(logradouro.Cep);
        Assert.True(existe);

        var existeMesmoId =
            await _repository.CepJaExiste(logradouro.Cep, logradouro.Id);

        Assert.False(existeMesmoId);

        var cepInedito = Cep.Criar(GerarCep()).Value!;
        var existeInedito = await _repository.CepJaExiste(cepInedito);

        Assert.False(existeInedito);
=======
        await _repository.Adicionar(logradouro);

        var cepCriado = Cep.Criar(cep).Value!;
        var resultado = await _repository.ObterPorCep(cepCriado);

        Assert.NotNull(resultado);
        Assert.Equal(cep, resultado.Cep.ToString());

        var cepInexistente = Cep.Criar("99999998").Value!;
        Assert.Null(await _repository.ObterPorCep(cepInexistente));
    }

    [Fact]
    public async Task Logradouro_CepJaExiste_ValidacaoCorreta()
    {
        var cep = GerarCep();

        var logradouro = Logradouro.Criar(
            0, cep, "Rua CEP", "Centro",
            "Lages", "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var cepCriado = Cep.Criar(cep).Value!;

        Assert.True(await _repository.CepJaExiste(cepCriado));
>>>>>>> 91e6a53 (Adiciona Testes em Logradouro em Atv 05)
    }

    [Fact]
    public async Task Logradouro_ObterPorCidade_FiltragemCorreta()
    {
<<<<<<< HEAD
        var cep = GerarCep();
        var cidadeUnica = "CidadeUnica_" + Guid.NewGuid().ToString("N")[..5];

        var logradouro = Logradouro.Criar(
            0, cep, "Rua X", "Bairro Y",
=======
        var cidadeUnica =
            "Cidade_" + Guid.NewGuid().ToString("N")[..5];

        var logradouro = Logradouro.Criar(
            0, GerarCep(), "Rua X", "Bairro Y",
>>>>>>> 91e6a53 (Adiciona Testes em Logradouro em Atv 05)
            cidadeUnica, "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var resultados =
            await _repository.ObterPorCidade(cidadeUnica);

        Assert.NotNull(resultados);
        Assert.Single(resultados);
        Assert.Equal(cidadeUnica, resultados.First().Cidade);

        var resultadosVazio =
            await _repository.ObterPorCidade("CidadeInexistente_123");

        Assert.Empty(resultadosVazio);
    }

    [Fact]
    public async Task Logradouro_ObterPorBairro_FiltragemCorreta()
    {
        var cep = GerarCep();
<<<<<<< HEAD
        var cidade = "Cidade_" + Guid.NewGuid().ToString("N")[..5];
        var bairro = "Bairro_" + Guid.NewGuid().ToString("N")[..5];
=======
        var cidade =
            "Cidade_" + Guid.NewGuid().ToString("N")[..5];
        var bairro =
            "Bairro_" + Guid.NewGuid().ToString("N")[..5];
>>>>>>> 91e6a53 (Adiciona Testes em Logradouro em Atv 05)

        var logradouro = Logradouro.Criar(
            0, cep, "Rua Z", bairro,
            cidade, "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var resultados =
            await _repository.ObterPorBairro(cidade, bairro);

        Assert.NotNull(resultados);
        Assert.Single(resultados);

        var resultadosVazio =
<<<<<<< HEAD
            await _repository.ObterPorBairro(cidade, "BairroInexistente");
=======
            await _repository.ObterPorBairro(
                cidade, "BairroInexistente");
>>>>>>> 91e6a53 (Adiciona Testes em Logradouro em Atv 05)

        Assert.Empty(resultadosVazio);
    }
}