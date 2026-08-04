namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public class Telefone
{
    public string Numero { get; protected set; }

    public Telefone(string numero)
    {
        Numero = numero;
    }
}