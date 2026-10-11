using Avalonia.Controls;
using Avalonia.Input;

namespace TaskManager.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        this.FindControl<ListBox>("TasksList")?.Focus();
    }

    private void OnTitleInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter)
        {
            return;
        }

        e.Handled = true;

        if (DataContext is ViewModels.MainViewModel viewModel && viewModel.AddCommand.CanExecute(null))
        {
            viewModel.AddCommand.Execute(null);
        }
    }
}