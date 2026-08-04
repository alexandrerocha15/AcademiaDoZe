namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public class Senha
{
    public string Valor { get; protected set; }

    public Senha(string valor)
    {
        Valor = valor;
    }
}