namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public class Arquivo
{
    public string Nome { get; protected set; }

    public Arquivo(string nome)
    {
        Nome = nome;
    }
}