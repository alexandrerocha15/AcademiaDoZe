using AcademiaDoZe.Application.DTOs;
namespace AcademiaDoZe.Application.Interfaces;
public interface IAcessoAlunoService
{
    Task<ResultadoAcessoDto> RegistrarAsync(int alunoId, CancellationToken cancellationToken = default);
}
