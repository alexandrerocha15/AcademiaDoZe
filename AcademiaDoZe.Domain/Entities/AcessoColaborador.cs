namespace AcademiaDoZe.Domain.Entities;

// Aluno: Alexandre Rocha

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }
    public DateTime DataHora { get; private set; }

    private AcessoColaborador(
        int id,
        Colaborador colaborador,
        DateTime dataHora)
        : base(id)
    {
        Colaborador = colaborador;
        DataHora = dataHora;
    }
}