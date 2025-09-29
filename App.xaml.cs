using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;
using DynaNoty.Services;

namespace DynaNoty
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private ILogger<App> _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Настройка DI контейнера
            var services = new ServiceCollection();
            services.AddDynaNotyServices(LogLevel.Debug);
            _serviceProvider = services.BuildServiceProvider();

            // Регистрируем все обработчики типов уведомлений
            var registryService = _serviceProvider.GetRequiredService<NotificationTypeHandlerRegistryService>();
            registryService.RegisterAllHandlers();

            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            _logger.LogInformation("DynaNoty приложение запущено");

            // Создание главного окна через DI
            var mainWindow = new MainWindow(
                _serviceProvider.GetRequiredService<INotificationService>(),
                _serviceProvider.GetRequiredService<ILogger<MainWindow>>(),
                _serviceProvider);
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger?.LogInformation("DynaNoty приложение завершается");

            // Освобождение ресурсов
            _serviceProvider?.Dispose();

            base.OnExit(e);
        }
    }
}
