using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    private LogradouroRepository CriarLogradouroRepository() =>
        new(ConnectionString, DatabaseType);

    private AlunoRepository CriarAlunoRepository() =>
        new(ConnectionString, DatabaseType);

    private MatriculaRepository CriarRepository() =>
        new(ConnectionString, DatabaseType);

    private string NomeBanco => DatabaseType.ToString();

    private async Task<Aluno> CriarEInserirAlunoAsync()
    {
        var logradouroRepository = CriarLogradouroRepository();
        var alunoRepository = CriarAlunoRepository();

        var logradouro = Logradouro.Criar(
            0,
            Random.Shared.Next(10000000, 99999999).ToString(),
            "Rua Teste",
            "Centro",
            "Lages",
            "SC",
            "Brasil"
        ).Value!;

        logradouro = await logradouroRepository.Adicionar(logradouro);

        var aluno = Aluno.Criar(
            0,
            "Alexandre",
            GerarCpf(),
            new DateOnly(1995, 5, 15),
            GerarTelefone(),
            GerarEmail(),
            logradouro,
            "200",
            "Rocha",
            SenhaDoBanco,
            Arquivo.Criar([1, 2, 3]).Value!
        ).Value!;

        return await alunoRepository.Adicionar(aluno);
    }

    private async Task<Matricula> CriarEInserirMatriculaAsync(
        MatriculaPlano plano = MatriculaPlano.Mensal)
    {
        var aluno = await CriarEInserirAlunoAsync();

        var matricula = Matricula.Criar(
            0,
            aluno,
            plano,
            DateOnly.FromDateTime(DateTime.Today),
            "Alexandre",
            MatriculaRestricoes.None,
            null,
            NomeBanco
        ).Value!;

        return await CriarRepository().Adicionar(matricula);
    }

    [Fact]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var repository = CriarRepository();
        var matricula = await CriarEInserirMatriculaAsync();

        var obtida = await repository.ObterPorId(matricula.Id);

        Assert.NotNull(obtida);
        Assert.Equal(matricula.Id, obtida.Id);
        Assert.Equal("Alexandre", obtida.Objetivo);
    }

    [Fact]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var repository = CriarRepository();

        var matricula = await repository.ObterPorId(int.MaxValue);

        Assert.Null(matricula);
    }

    [Fact]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        var repository = CriarRepository();
        var matricula = await CriarEInserirMatriculaAsync();

        var matriculas = await repository.ObterTodos();

        Assert.Contains(matriculas, x => x.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var repository = CriarRepository();
        var aluno = await CriarEInserirAlunoAsync();

        var inserida = Matricula.Criar(
            id: 0,
            aluno: aluno,
            plano: MatriculaPlano.Mensal,
            dataInicio: DateOnly.FromDateTime(DateTime.Today),
            objetivo: "Alexandre",
            restricoesMedicas: MatriculaRestricoes.None,
            laudoMedico: null,
            observacoesRestricoes: NomeBanco
        ).Value!;

        inserida = await repository.Adicionar(inserida);

        var matriculaAtualizada = Matricula.Criar(
            id: inserida.Id,
            aluno: aluno,
            plano: MatriculaPlano.Anual,
            dataInicio: inserida.DataInicio,
            objetivo: "Alexandre",
            restricoesMedicas: MatriculaRestricoes.None,
            laudoMedico: null,
            observacoesRestricoes: NomeBanco
        ).Value!;

        await repository.Atualizar(matriculaAtualizada);

        var noBanco = await repository.ObterPorId(inserida.Id);

        Assert.NotNull(noBanco);
        Assert.Equal("Alexandre", noBanco.Objetivo);
        Assert.Equal(MatriculaPlano.Anual, noBanco.Plano);
    }

    [Fact]
    public async Task Matricula_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var matricula = Matricula.Criar(
            int.MaxValue,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Alexandre",
            MatriculaRestricoes.None,
            null,
            NomeBanco
        ).Value!;

        var repository = CriarRepository();

        await Assert.ThrowsAsync<InfrastructureException>(
            () => repository.Atualizar(matricula));
    }

    [Fact]
    public async Task Matricula_Remover_Sucesso()
    {
        var repository = CriarRepository();
        var matricula = await CriarEInserirMatriculaAsync();

        var resultado = await repository.Remover(matricula.Id);
        var obtida = await repository.ObterPorId(matricula.Id);

        Assert.True(resultado);
        Assert.Null(obtida);
    }

    [Fact]
    public async Task Matricula_Remover_RetornaFalseQuandoInexistente()
    {
        var repository = CriarRepository();

        var resultado = await repository.Remover(int.MaxValue);

        Assert.False(resultado);
    }

    [Fact]
    public async Task Matricula_ObterPorAluno_Sucesso()
    {
        var repository = CriarRepository();
        var matricula = await CriarEInserirMatriculaAsync();

        var matriculas = await repository.ObterPorAluno(matricula.AlunoId);

        Assert.Contains(matriculas, x => x.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_Sucesso()
    {
        var repository = CriarRepository();
        var matricula = await CriarEInserirMatriculaAsync();

        var ativa =
            await repository.ObterMatriculaAtivaPorAluno(matricula.AlunoId);

        Assert.NotNull(ativa);
        Assert.Equal(matricula.Id, ativa.Id);
    }

    [Fact]
    public async Task Matricula_PossuiMatriculaAtiva_RetornaTrue()
    {
        var repository = CriarRepository();
        var matricula = await CriarEInserirMatriculaAsync();

        var possui =
            await repository.PossuiMatriculaAtiva(matricula.AlunoId);

        Assert.True(possui);
    }

    [Fact]
    public async Task Matricula_ObterAtivas_Sucesso()
    {
        var repository = CriarRepository();
        var matricula = await CriarEInserirMatriculaAsync();

        var ativas = await repository.ObterAtivas();

        Assert.Contains(ativas, x => x.Id == matricula.Id);
    }

    [Fact]
    public async Task Matricula_ObterVencendoEmDias_Sucesso()
    {
        var repository = CriarRepository();
        await CriarEInserirMatriculaAsync();

        var matriculas = await repository.ObterVencendoEmDias(365);

        Assert.NotNull(matriculas);
    }

    [Fact]
    public async Task Matricula_ObterPorPlano_Sucesso()
    {
        var repository = CriarRepository();

        var matricula =
            await CriarEInserirMatriculaAsync(MatriculaPlano.Mensal);

        var matriculas =
            await repository.ObterPorPlano(MatriculaPlano.Mensal);

        Assert.Contains(matriculas, x => x.Id == matricula.Id);
    }
}