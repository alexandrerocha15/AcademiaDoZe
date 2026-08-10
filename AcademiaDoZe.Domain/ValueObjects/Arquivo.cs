namespace AcademiaDoZe.Domain.ValueObjects;

// Aluno: Alexandre Rocha

public record Arquivo
{
    public byte[] Conteudo { get; }

    private Arquivo(byte[] conteudo)
    {
        Conteudo = conteudo;
    }
}