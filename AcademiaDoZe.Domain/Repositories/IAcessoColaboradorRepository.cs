using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

// Aluno: Alexandre Rocha

public interface IAcessoColaboradorRepository : IRepository<AcessoColaborador>
{
    Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(
        int? colaboradorId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default);

    Task<AcessoColaborador?> ObterUltimoAcesso(
        int colaboradorId,
        CancellationToken cancellationToken = default);

    Task<TimeSpan> ObterHorasTrabalhadasNoDia(
        int colaboradorId,
        DateOnly data,
        CancellationToken cancellationToken = default);
}