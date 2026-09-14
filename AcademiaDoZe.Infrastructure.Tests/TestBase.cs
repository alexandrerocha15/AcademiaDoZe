using AcademiaDoZe.Infrastructure.Data;
using Xunit;

[assembly: CollectionBehavior(
    CollectionBehavior.CollectionPerAssembly,
    DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    // Por enquanto começamos com SQLite
    private const DatabaseType SelectedDatabaseType = DatabaseType.Sqlite;
    protected static string SenhaDoBanco => SelectedDatabaseType switch
    {
        DatabaseType.Sqlite => "SenhaSQLite123",
        DatabaseType.SqlServer => "SenhaSQLServer123",
        DatabaseType.MySql => "SenhaMySQL123",
        _ => "SenhaValida123"
    };

    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        ConnectionString = DatabaseType switch
        {
            DatabaseType.SqlServer =>
                "Server=localhost;Database=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=True;",

            DatabaseType.MySql =>
                "Server=localhost;Port=3307;Database=db_academia_do_ze;User Id=root;Password=abcBolinhas12345;",

            DatabaseType.Sqlite =>
                @"Data Source=db_academia_do_ze.db;Cache=Shared;",

            _ => throw new ArgumentOutOfRangeException(
                nameof(DatabaseType),
                DatabaseType,
                "SGBD não suportado para testes.")
        };
    }
    protected static string GerarCpf()
    {
        var random = Random.Shared;

        int[] numeros = new int[11];

        for (int i = 0; i < 9; i++)
            numeros[i] = random.Next(0, 10);

        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += numeros[i] * (10 - i);

        int resto = soma % 11;
        numeros[9] = resto < 2 ? 0 : 11 - resto;

        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += numeros[i] * (11 - i);

        resto = soma % 11;
        numeros[10] = resto < 2 ? 0 : 11 - resto;

        return string.Concat(numeros);
    }

    protected static string GerarTelefone()
    {
        return $"49{Random.Shared.Next(900000000, 999999999)}";
    }

    protected static string GerarEmail()
    {
        return $"alexandre.{Guid.NewGuid():N}@teste.com";
    }
}