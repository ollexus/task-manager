using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManager.Desktop.ViewModels;
using TaskManager.Desktop.Views;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Desktop;

public partial class App : Application
{
    private IServiceScope? _scope;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                Start(desktop);
            }
            catch (Exception exception)
            {
                _scope?.ServiceProvider.GetRequiredService<ILogger<App>>()
                    .LogCritical(exception, "Не удалось инициализировать приложение");

                desktop.MainWindow = new StartupErrorWindow(exception.Message);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Start(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString(TaskManagerDbContext.ConnectionStringName)
                               ?? throw new InvalidOperationException(
                                   $"Не задана строка подключения '{TaskManagerDbContext.ConnectionStringName}' в appsettings.json.");

        var services = new ServiceCollection()
            .AddLogging()
            .AddDbContext<TaskManagerDbContext>(options => options.UseNpgsql(connectionString))
.AddScoped<ITaskRepository, TaskRepository>()
                .AddScoped<MainViewModel>()
            .BuildServiceProvider();

        _scope = services.CreateScope();
        _scope.ServiceProvider.GetRequiredService<TaskManagerDbContext>().Database.Migrate();

        var viewModel = _scope.ServiceProvider.GetRequiredService<MainViewModel>();

        desktop.MainWindow = new MainWindow { DataContext = viewModel };
        desktop.MainWindow.Opened += async (_, _) => await viewModel.LoadCommand.ExecuteAsync(null);
        desktop.Exit += (_, _) => _scope.Dispose();
    }
}