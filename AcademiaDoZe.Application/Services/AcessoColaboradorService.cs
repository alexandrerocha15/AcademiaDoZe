using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _acessos;
    private readonly Func<IColaboradorRepository> _colaboradores;

    public AcessoColaboradorService(Func<IAcessoColaboradorRepository> acessos,
        Func<IColaboradorRepository> colaboradores)
    {
        _acessos = acessos;
        _colaboradores = colaboradores;
    }

    public async Task<ResultadoAcessoDto> RegistrarAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradores().ObterPorId(colaboradorId, cancellationToken)
            ?? throw new InvalidOperationException("Colaborador não encontrado.");
        var repositorio = _acessos();
        var registros = (await repositorio.ObterAcessosPorColaboradorPeriodo(colaboradorId, cancellationToken: cancellationToken))
            .OrderBy(a => a.DataHora).ThenBy(a => a.Id).ToList();
        bool entrada = registros.Count % 2 == 0;
        var agora = DateTime.Now;
        var data = DateOnly.FromDateTime(agora);
        TimeSpan duracao;

        if (entrada)
        {
            var total = await repositorio.ObterHorasTrabalhadasNoDia(colaboradorId, data, cancellationToken);
            var limite = colaborador.Vinculo == ColaboradorVinculo.CLT
                ? TimeSpan.FromHours(8) : TimeSpan.FromHours(6);
            if (total >= limite)
                throw new InvalidOperationException("Limite diário de horas trabalhadas atingido.");
            duracao = TimeSpan.Zero;
        }
        else
        {
            var inicio = registros[^1].DataHora;
            var totalAnterior = await repositorio.ObterHorasTrabalhadasNoDia(colaboradorId, data, cancellationToken);
            duracao = totalAnterior + (agora - inicio);
        }

        var acesso = AcessoColaborador.Criar(0, colaborador, agora);
        if (acesso.IsFailure)
            throw new InvalidOperationException(string.Join(", ", acesso.Notifications.Select(n => n.Mensagem)));
        await repositorio.Adicionar(acesso.Value!, cancellationToken);
        return new ResultadoAcessoDto(entrada, agora, duracao);
    }
}
