using AcademiaDoZe.Infrastructure.Data;
using Xunit;

[assembly: CollectionBehavior(
    CollectionBehavior.CollectionPerAssembly,
    DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    // Por enquanto começamos com SQLite
    private const DatabaseType SelectedDatabaseType = DatabaseType.SqlServer;

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
}