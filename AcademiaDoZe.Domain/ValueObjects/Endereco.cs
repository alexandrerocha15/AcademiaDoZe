namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public class Endereco
{
    public string Logradouro { get; protected set; }

    public Endereco(string logradouro)
    {
        Logradouro = logradouro;
    }
}