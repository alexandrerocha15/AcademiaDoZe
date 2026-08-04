using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

// Aluno: Alexandre Rocha

public abstract class Pessoa : Entity
{
    public string Nome { get; protected set; }
    public Cpf Cpf { get; protected set; }
    public Email Email { get; protected set; }
    public Telefone Telefone { get; protected set; }
    public Endereco Endereco { get; protected set; }

    protected Pessoa(
        int id,
        string nome,
        Cpf cpf,
        Email email,
        Telefone telefone,
        Endereco endereco)
        : base(id)
    {
        Nome = nome;
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
        Endereco = endereco;
    }
}