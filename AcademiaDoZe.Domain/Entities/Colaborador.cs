using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

// Aluno: Alexandre Rocha

public class Colaborador : Pessoa
{
    public Colaborador(
        int id,
        string nome,
        Cpf cpf,
        Email email,
        Telefone telefone,
        Endereco endereco)
        : base(id, nome, cpf, email, telefone, endereco)
    {
    }
}