using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _acessos;
    private readonly Func<IAlunoRepository> _alunos;
    private readonly Func<IMatriculaRepository> _matriculas;

    public AcessoAlunoService(Func<IAcessoAlunoRepository> acessos,
        Func<IAlunoRepository> alunos, Func<IMatriculaRepository> matriculas)
    {
        _acessos = acessos;
        _alunos = alunos;
        _matriculas = matriculas;
    }

    public async Task<ResultadoAcessoDto> RegistrarAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var aluno = await _alunos().ObterPorId(alunoId, cancellationToken)
            ?? throw new InvalidOperationException("Aluno não encontrado.");
        var repositorio = _acessos();
        var registros = (await repositorio.ObterAcessosPorAlunoPeriodo(alunoId, cancellationToken: cancellationToken))
            .OrderBy(a => a.DataHora).ThenBy(a => a.Id).ToList();
        bool entrada = registros.Count % 2 == 0;
        var agora = DateTime.Now;
        TimeSpan duracao;

        if (entrada)
        {
            var matricula = await _matriculas().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken)
                ?? throw new InvalidOperationException("Aluno sem matrícula ativa.");
            duracao = matricula.DataFim.ToDateTime(TimeOnly.MinValue).Date.AddDays(1) - agora;
        }
        else
        {
            duracao = agora - registros[^1].DataHora;
        }

        var acesso = AcessoAluno.Criar(0, aluno, agora);
        if (acesso.IsFailure)
            throw new InvalidOperationException(string.Join(", ", acesso.Notifications.Select(n => n.Mensagem)));
        await repositorio.Adicionar(acesso.Value!, cancellationToken);
        return new ResultadoAcessoDto(entrada, agora, duracao);
    }
}
