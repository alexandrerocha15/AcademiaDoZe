using AcademiaDoZe.Application.DTOs;
namespace AcademiaDoZe.Application.Interfaces;
public interface IAcessoColaboradorService
{
    Task<ResultadoAcessoDto> RegistrarAsync(int colaboradorId, CancellationToken cancellationToken = default);
}
