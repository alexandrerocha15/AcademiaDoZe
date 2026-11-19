using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private const int PessoaTipo = 0;
    private readonly AlunoRepository _pessoaRepository;

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
        _pessoaRepository = new AlunoRepository(connectionString, databaseType);
    }

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        int pessoaId;
        DateTime dataHora;
        await using (var command = await CreateCommandAsync("SELECT pessoa_id, data_hora FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @Tipo", cancellationToken))
        {
            command.AddParameter("@Id", id, DbType.Int32);
            command.AddParameter("@Tipo", PessoaTipo, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;
            pessoaId = reader.GetInt32Value("pessoa_id");
            dataHora = reader.GetDateTimeValue("data_hora");
        }
        var pessoa = await _pessoaRepository.ObterPorId(pessoaId, cancellationToken)
            ?? throw new InfrastructureException("PESSOA_NAO_ENCONTRADA", "Pessoa do acesso não encontrada.");
        var result = AcessoAluno.Criar(id, pessoa, dataHora);
        if (result.IsFailure)
            throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        var registros = new List<(int Id, int PessoaId, DateTime DataHora)>();
        await using (var command = await CreateCommandAsync("SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = @Tipo ORDER BY data_hora, id_acesso", cancellationToken))
        {
            command.AddParameter("@Tipo", PessoaTipo, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                registros.Add((reader.GetInt32Value("id_acesso"), reader.GetInt32Value("pessoa_id"), reader.GetDateTimeValue("data_hora")));
        }
        var resultado = new List<AcessoAluno>();
        foreach (var registro in registros)
        {
            var pessoa = await _pessoaRepository.ObterPorId(registro.PessoaId, cancellationToken)
                ?? throw new InfrastructureException("PESSOA_NAO_ENCONTRADA", "Pessoa do acesso não encontrada.");
            var acesso = AcessoAluno.Criar(registro.Id, pessoa, registro.DataHora);
            if (acesso.IsFailure)
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", string.Join(", ", acesso.Notifications.Select(n => n.Mensagem)));
            resultado.Add(acesso.Value!);
        }
        return resultado;
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        var query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@Tipo, @PessoaId, @DataHora)");
        await using var command = await CreateCommandAsync(query, cancellationToken);
        command.AddParameter("@Tipo", PessoaTipo, DbType.Int32);
        command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
        command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO", "Falha ao obter ID do acesso.", cancellationToken);
        typeof(Entity).GetProperty("Id")?.SetValue(entity, id);
        return entity;
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @Tipo", cancellationToken);
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        command.AddParameter("@Tipo", PessoaTipo, DbType.Int32);
        command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
        command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
            throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", "Acesso não encontrado para atualização.");
        return entity;
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @Tipo", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        command.AddParameter("@Tipo", PessoaTipo, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var todos = await ObterTodos(cancellationToken);
        return todos.Where(a => (!alunoId.HasValue || a.AlunoId == alunoId.Value)
            && (!inicio.HasValue || DateOnly.FromDateTime(a.DataHora) >= inicio.Value)
            && (!fim.HasValue || DateOnly.FromDateTime(a.DataHora) <= fim.Value));
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        var acessos = await ObterAcessosPorAlunoPeriodo(alunoId, cancellationToken: cancellationToken);
        return acessos.OrderByDescending(a => a.DataHora).ThenByDescending(a => a.Id).FirstOrDefault();
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        var acessos = await ObterAcessosPorAlunoPeriodo(alunoId, cancellationToken: cancellationToken);
        return acessos.Count() % 2 != 0;
    }
}
