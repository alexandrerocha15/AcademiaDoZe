namespace AcademiaDoZe.Domain.Exceptions;

// Aluno: Alexandre Rocha

public sealed class DomainException(string message) : Exception(message)
{
}