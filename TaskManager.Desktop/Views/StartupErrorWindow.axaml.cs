using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

namespace TaskManager.Desktop.Views;

public partial class StartupErrorWindow : Window
{
    public StartupErrorWindow()
    {
        InitializeComponent();
    }

    public StartupErrorWindow(string message) : this()
    {
        this.FindControl<TextBlock>("ErrorText")!.Text = message;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}