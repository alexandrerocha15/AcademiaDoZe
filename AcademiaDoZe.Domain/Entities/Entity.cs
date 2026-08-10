using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities;

// Aluno: Alexandre Rocha

public abstract class Entity
{
    public int Id { get; protected set; }

    protected Entity(int id = 0)
    {
        if (id < 0)
            throw new DomainException("ID_NEGATIVO");

        Id = id;
    }
}