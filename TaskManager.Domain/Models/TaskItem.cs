namespace TaskManager.Domain.Models;

public class TaskItem
{
    public const int TitleMaxLength = 100;
    
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new ArgumentException("Title cannot be empty", nameof(Title));
        }

        if (Title.Trim().Length > TitleMaxLength)
        {
            throw new ArgumentException("Title cannot be longer than " + TitleMaxLength, nameof(Title));
        }
    }
}