using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

// Aluno: Alexandre Rocha

public sealed class Logradouro : Entity, IAggregateRoot
{
    public Cep Cep { get; }
    public string Nome { get; }
    public string Bairro { get; }
    public string Cidade { get; }
    public string Estado { get; }
    public string Pais { get; }

    private Logradouro(
        int id,
        Cep cep,
        string nome,
        string bairro,
        string cidade,
        string estado,
        string pais) : base(id)
    {
        Cep = cep;
        Nome = nome;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Pais = pais;
    }

    public static Result<Logradouro> Criar(
        int id,
        string cep,
        string nome,
        string bairro,
        string cidade,
        string estado,
        string pais)
    {
        var notifications = new List<Notification>();

        var cepResult = Cep.Criar(cep);

        if (cepResult.IsFailure)
            notifications.AddRange(cepResult.Notifications);

        if (NormalizacaoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));

        if (NormalizacaoService.TextoVazioOuNulo(bairro))
            notifications.Add(new Notification("Bairro", "BAIRRO_OBRIGATORIO"));

        if (NormalizacaoService.TextoVazioOuNulo(cidade))
            notifications.Add(new Notification("Cidade", "CIDADE_OBRIGATORIO"));

        if (NormalizacaoService.TextoVazioOuNulo(estado))
            notifications.Add(new Notification("Estado", "ESTADO_OBRIGATORIO"));

        if (NormalizacaoService.TextoVazioOuNulo(pais))
            notifications.Add(new Notification("Pais", "PAIS_OBRIGATORIO"));

        if (notifications.Count != 0)
            return Result<Logradouro>.Failure(notifications);

        nome = NormalizacaoService.LimparEspacos(nome);
        bairro = NormalizacaoService.LimparEspacos(bairro);
        cidade = NormalizacaoService.LimparEspacos(cidade);
        estado = NormalizacaoService.LimparEspacos(estado);
        pais = NormalizacaoService.LimparEspacos(pais);

        return Result<Logradouro>.Success(
            new Logradouro(
                id,
                cepResult.Value!,
                nome,
                bairro,
                cidade,
                estado,
                pais));
    }
}