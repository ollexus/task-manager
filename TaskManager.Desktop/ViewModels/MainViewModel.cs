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
    private string _newTaskTitle = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<TaskItem> Tasks { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        await RunGuardedAsync(async () =>
        {
            var items = await _repository.GetAllAsync();

            Tasks.Clear();
            foreach (var item in items)
            {
                Tasks.Add(item);
            }
        });
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle))
        {
            return;
        }

        var title = NewTaskTitle;

        await RunGuardedAsync(async () =>
        {
            var task = new TaskItem { Title = title };

            await _repository.AddAsync(task);

            Tasks.Add(task);
            NewTaskTitle = string.Empty;
        });
    }

    [RelayCommand]
    private async Task ToggleCompletedAsync(TaskItem? task)
    {
        if (task is null)
        {
            return;
        }

        var previousValue = task.IsCompleted;

        await RunGuardedAsync(async () =>
        {
            task.IsCompleted = !previousValue;
            await _repository.UpdateAsync(task);
        }, () => task.IsCompleted = previousValue);
    }

    [RelayCommand]
    private async Task DeleteAsync(TaskItem? task)
    {
        if (task is null)
        {
            return;
        }

        await RunGuardedAsync(async () =>
        {
            await _repository.DeleteAsync(task);
            Tasks.Remove(task);
        });
    }

    private async Task RunGuardedAsync(Func<Task> action, Action? onFailure = null)
    {
        if (IsBusy)
        {
            return;
        }

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
        }
    }
}