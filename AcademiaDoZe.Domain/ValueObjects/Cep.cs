namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public class Cep
{
    public string Numero { get; protected set; }

    public Cep(string numero)
    {
        Numero = numero;
    }
}