using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Models;

namespace TaskManager.Infrastructure;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks", table =>
        {
            table.HasCheckConstraint("ck_tasks_title_not_blank", """btrim("Title") <> ''""");
        });
        
        builder.HasKey(task => task.Id);

        builder.HasQueryFilter(task => !task.IsDeleted);

        builder.Property(task => task.Id)
            .ValueGeneratedNever();

        builder.Property(task => task.Title)
            .HasMaxLength(TaskItem.TitleMaxLength)
            .IsRequired();

        builder.Property(task => task.IsCompleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(task => task.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(task => task.IsDeleted);

        builder.Property(task => task.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .ValueGeneratedNever()
            .IsRequired();
    }
}