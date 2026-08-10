namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public record Senha
{
    public string Valor { get; }

    private Senha(string valor)
    {
        Valor = valor;
    }
}