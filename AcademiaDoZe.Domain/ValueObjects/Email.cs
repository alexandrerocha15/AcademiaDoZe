namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public class Email
{
    public string Endereco { get; protected set; }

    public Email(string endereco)
    {
        Endereco = endereco;
    }
}