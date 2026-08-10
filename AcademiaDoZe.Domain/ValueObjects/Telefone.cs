namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public record Telefone
{
    public string Valor { get; }

    private Telefone(string valor)
    {
        Valor = valor;
    }
}