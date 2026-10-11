using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManager.Infrastructure;

public sealed class TaskManagerDbContextFactory : IDesignTimeDbContextFactory<TaskManagerDbContext>
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=taskdb;Username=postgres;Password=postgres";

    public TaskManagerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new TaskManagerDbContext(options);
    }
}