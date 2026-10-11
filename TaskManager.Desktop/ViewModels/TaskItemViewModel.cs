using CommunityToolkit.Mvvm.ComponentModel;
using TaskManager.Domain.Models;

namespace TaskManager.Desktop.ViewModels;

public partial class TaskItemViewModel(TaskItem model, Func<TaskItemViewModel, Task> onCompletedChanged) : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateText))]
    private bool _isCompleted = model.IsCompleted;

    public TaskItem Model { get; } = model;

    public Guid Id => Model.Id;

    public string Title => Model.Title;

    public DateTime CreatedAt => Model.CreatedAt;

    public string CreatedAtText => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    public string StateText => IsCompleted ? "Выполнена" : "Активная";

    partial void OnIsCompletedChanged(bool value)
    {
        Model.IsCompleted = value;
        _ = onCompletedChanged(this);
    }
}