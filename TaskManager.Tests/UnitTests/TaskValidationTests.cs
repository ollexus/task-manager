using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Domain.Models;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Tests.Infrastructure;

namespace TaskManager.Tests.UnitTests;

[Collection(PostgresCollection.Name)]
public sealed class TaskValidationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task AddAsync_WithBlankTitle_DoesNotPersist()
    {
        await using var context = fixture.CreateContext();
        var repository = new TaskRepository(context, NullLogger<TaskRepository>.Instance);

        var before = (await repository.GetAllAsync()).Count;

        await Assert.ThrowsAsync<ArgumentException>(
            () => repository.AddAsync(new TaskItem { Title = "" }));

        var after = await repository.GetAllAsync();

        Assert.Equal(before, after.Count);
    }

    [Fact]
    public void Validate_WithTitleLongerThanMaxLength_Throws()
    {
        var task = new TaskItem { Title = new string('a', TaskItem.TitleMaxLength + 1) };

        Assert.Throws<ArgumentException>(task.Validate);
    }
}