namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public record Cpf
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }
}