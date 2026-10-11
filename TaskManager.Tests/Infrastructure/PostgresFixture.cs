using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskManager.Infrastructure;

namespace TaskManager.Tests.Infrastructure;

public sealed class PostgresFixture : IAsyncLifetime
{
    private const string AdminConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";

    private string _databaseName = string.Empty;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _databaseName = $"taskdb_tests_{Guid.NewGuid():N}";
        ConnectionString =
            $"Host=localhost;Port=5432;Database={_databaseName};Username=postgres;Password=postgres";

        await using (var connection = new NpgsqlConnection(AdminConnectionString))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
    }

    public TaskManagerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new TaskManagerDbContext(options);
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}