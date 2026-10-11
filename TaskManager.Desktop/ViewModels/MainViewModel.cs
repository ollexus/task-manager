using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Models;

namespace TaskManager.Desktop.ViewModels;

public partial class MainViewModel(ITaskRepository repository) : ViewModelBase
{
    private readonly ITaskRepository _repository = repository;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    private string _newTaskTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTasks))]
    private int _taskCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanToggleSelected))]
    private TaskItemViewModel? _selectedItem;

    public ObservableCollection<TaskItemViewModel> Tasks { get; } = [];

    public string? TitleValidationMessage { get; private set; }

    public bool HasTitleError => TitleValidationMessage is not null;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool HasTasks => TaskCount > 0;

    public bool CanAdd => !IsBusy;

    public bool CanToggleSelected => SelectedItem is not null;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private Task AddAsync()
    {
        if (!TryValidateTitle())
        {
            return Task.CompletedTask;
        }

        var title = NewTaskTitle.Trim();

        return RunGuardedAsync(async () =>
        {
            var task = new TaskItem { Title = title };

            await _repository.AddAsync(task);

            Tasks.Add(new TaskItemViewModel(task, PersistCompletedChangedAsync));
            TaskCount = Tasks.Count;

            NewTaskTitle = string.Empty;
            ClearTitleValidation();
        });
    }

    [RelayCommand]
    private Task LoadAsync() => RunGuardedAsync(async () =>
    {
        var items = await _repository.GetAllAsync();

        Tasks.Clear();
        foreach (var item in items)
        {
            Tasks.Add(new TaskItemViewModel(item, PersistCompletedChangedAsync));
        }

        TaskCount = Tasks.Count;
        SelectedItem = Tasks.FirstOrDefault();
    });

    [RelayCommand]
    private Task ToggleCompletedAsync(TaskItemViewModel? row)
    {
        if (row is null)
        {
            return Task.CompletedTask;
        }

        row.IsCompleted = !row.IsCompleted;

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeleteAsync(TaskItemViewModel? row)
    {
        if (row is null)
        {
            return;
        }

        await RunGuardedAsync(async () =>
        {
            await _repository.DeleteAsync(row.Model);
            Tasks.Remove(row);
            TaskCount = Tasks.Count;
        });
    }

    [RelayCommand]
    private Task DeleteSelectedAsync() => DeleteAsync(SelectedItem);

    [RelayCommand(CanExecute = nameof(CanToggleSelected))]
    private Task ToggleSelectedAsync()
    {
        if (SelectedItem is not { } row)
        {
            return Task.CompletedTask;
        }

        row.IsCompleted = !row.IsCompleted;

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        NewTaskTitle = string.Empty;
        ClearTitleValidation();
    }

    private Task PersistCompletedChangedAsync(TaskItemViewModel row)
    {
        var previousValue = !row.IsCompleted;

        return RunGuardedAsync(
            () => _repository.UpdateAsync(row.Model),
            () => row.IsCompleted = previousValue);
    }

    private bool TryValidateTitle()
    {
        TitleValidationMessage = string.IsNullOrWhiteSpace(NewTaskTitle)
            ? "Введите название задачи."
            : NewTaskTitle.Trim().Length > TaskItem.TitleMaxLength
                ? $"Название не может быть длиннее {TaskItem.TitleMaxLength} символов."
                : null;

        OnPropertyChanged(nameof(TitleValidationMessage));
        OnPropertyChanged(nameof(HasTitleError));

        return TitleValidationMessage is null;
    }

    private void ClearTitleValidation()
    {
        if (TitleValidationMessage is null)
        {
            return;
        }

        TitleValidationMessage = null;
        OnPropertyChanged(nameof(TitleValidationMessage));
        OnPropertyChanged(nameof(HasTitleError));
    }

    partial void OnNewTaskTitleChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            ClearTitleValidation();
        }
    }

    private async Task RunGuardedAsync(Func<Task> action, Action? onFailure = null)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            onFailure?.Invoke();
        }
        finally
        {
            IsBusy = false;
            AddCommand.NotifyCanExecuteChanged();
            ToggleSelectedCommand.NotifyCanExecuteChanged();
        }
    }
}