using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Domain.Models;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Tests.Infrastructure;

namespace TaskManager.Tests.UnitTests;

[Collection(PostgresCollection.Name)]
public sealed class RepositoryTests(PostgresFixture fixture)
{
    [Fact]
    public async Task AddAsync_PersistsTask()
    {
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context, NullLogger<TaskRepository>.Instance);

        await repository.AddAsync(new TaskItem { Title = "Persisted task" });

        var stored = await repository.GetAllAsync();

        Assert.Contains(stored, item => item.Title == "Persisted task");
    }

    [Fact]
    public async Task AddAsync_TrimsTitle()
    {
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context, NullLogger<TaskRepository>.Instance);

        await repository.AddAsync(new TaskItem { Title = "   Trimmed task   " });

        var stored = await repository.GetAllAsync();

        Assert.Contains(stored, item => item.Title == "Trimmed task");
    }

    [Fact]
    public async Task UpdateAsync_PersistsCompletedState()
    {
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context, NullLogger<TaskRepository>.Instance);

        var task = new TaskItem { Title = "Completed task" };
        await repository.AddAsync(task);

        task.IsCompleted = true;
        await repository.UpdateAsync(task);

        var stored = await repository.GetAllAsync();

        Assert.Contains(stored, item => item.Title == "Completed task" && item.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_KeepsTaskInDatabaseButHidesIt()
    {
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context, NullLogger<TaskRepository>.Instance);

        var task = new TaskItem { Title = "Soft deleted task" };
        await repository.AddAsync(task);

        await repository.DeleteAsync(task);

        Assert.DoesNotContain(await repository.GetAllAsync(), item => item.Title == "Soft deleted task");

        await using var rawContext = fixture.CreateContext();

        var rowCount = await rawContext.Tasks
            .IgnoreQueryFilters()
            .CountAsync(item => item.Title == "Soft deleted task");

        Assert.Equal(1, rowCount);
    }

    [Fact]
    public async Task DeleteAsync_AfterReloadOfUpdatedTask_DoesNotThrow()
    {
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context, NullLogger<TaskRepository>.Instance);

        var task = new TaskItem { Title = "Reloaded task" };
        await repository.AddAsync(task);

        task.IsCompleted = true;
        await repository.UpdateAsync(task);

        var stored = await repository.GetAllAsync();
        var freshCopy = stored.Single(item => item.Title == "Reloaded task");

        await repository.DeleteAsync(freshCopy);

        Assert.DoesNotContain(await repository.GetAllAsync(), item => item.Title == "Reloaded task");
    }
}