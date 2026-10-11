using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Infrastructure.Repositories;

public sealed class TaskRepository(TaskManagerDbContext context, ILogger<TaskRepository> logger) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Tasks
            .AsNoTracking()
            .OrderBy(task => task.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        Prepare(task);
        task.Validate();

        DetachTracked(task);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Добавлена задача {TaskId}", task.Id);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        task.Validate();

        DetachTracked(task);
        context.Tasks.Update(task);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Сохранена задача {TaskId}", task.Id);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        DetachTracked(task);

        task.IsDeleted = true;
        context.Tasks.Update(task);

        try
        {
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Задача {TaskId} удалена", task.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            context.ChangeTracker.Clear();

            logger.LogInformation("Задача {TaskId} уже была удалена", task.Id);
        }
    }
    
    private void DetachTracked(TaskItem task)
    {
        var tracked = context.Tasks.Local.FirstOrDefault(local => local.Id == task.Id);

        if (tracked is not null && !ReferenceEquals(tracked, task))
        {
            context.Entry(tracked).State = EntityState.Detached;
        }
    }
    
    private static void Prepare(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        task.Title = task.Title?.Trim() ?? string.Empty;
        task.Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id;
        task.CreatedAt = task.CreatedAt == default ? DateTime.UtcNow : task.CreatedAt;
    }
}