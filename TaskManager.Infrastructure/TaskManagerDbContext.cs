using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Models;

namespace TaskManager.Infrastructure;

public class TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public const string ConnectionStringName = "TaskDb";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TaskItemConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}