using TaskManager.Domain.Models;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default);
}