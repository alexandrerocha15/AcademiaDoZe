namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public class Cpf
{
    public string Numero { get; protected set; }

    public Cpf(string numero)
    {
        Numero = numero;
    }
}