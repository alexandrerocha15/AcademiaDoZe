using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Repositories;

// Aluno: Alexandre Rocha

public interface IColaboradorRepository : IRepository<Colaborador>
{
    Task<Colaborador?> ObterPorCpf(
        Cpf cpf,
        CancellationToken cancellationToken = default);

    Task<Colaborador?> ObterPorEmail(
        Email email,
        CancellationToken cancellationToken = default);

    Task<bool> CpfJaExiste(
        Cpf cpf,
        int? id = null,
        CancellationToken cancellationToken = default);

    Task<bool> EmailJaExiste(
        Email email,
        int? id = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Colaborador>> ObterPorTipo(
        ColaboradorTipo tipo,
        CancellationToken cancellationToken = default);
}