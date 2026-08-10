namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public record Email
{
    public string Valor { get; }

    private Email(string valor)
    {
        Valor = valor;
    }
}