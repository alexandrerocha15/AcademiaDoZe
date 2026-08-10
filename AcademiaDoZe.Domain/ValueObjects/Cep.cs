namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public record Cep
{
    public string Valor { get; }

    private Cep(string valor)
    {
        Valor = valor;
    }
}