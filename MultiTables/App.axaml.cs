using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MultiTables.Services;
using MultiTables.ViewModels;
using MultiTables.Views;

namespace MultiTables;

public partial class App : Application
{
    public new static App? Current => Application.Current as App;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // сервисы
        services.AddSingleton<IImageGeneratorService, ImageGeneratorService>();

        // viewmodels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<PreviewViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVM = serviceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVM
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}