using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;
using DynaNoty.Configuration;

namespace DynaNoty.Services
{
    /// <summary>
    /// Расширения для регистрации сервисов в DI контейнере
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует все сервисы уведомлений
        /// </summary>
        public static IServiceCollection AddNotificationServices(this IServiceCollection services, NotificationConfiguration config = null)
        {
            // Регистрируем конфигурацию
            services.AddSingleton(config ?? new NotificationConfiguration());

            // Регистрируем основные сервисы
            services.AddSingleton<INotificationWindow, NotificationWindow>();
            services.AddSingleton<ISystemThemeService, SystemThemeService>();
            services.AddSingleton<IInputValidationService>(provider =>
            {
                var config = provider.GetRequiredService<NotificationConfiguration>();
                var logger = provider.GetService<ILogger<InputValidationService>>();
                return new InputValidationService(config, logger);
            });
            services.AddSingleton<INotificationPositioningService, NotificationPositioningService>();
            services.AddSingleton<IAnimationOptimizer, AnimationOptimizer>();
            services.AddSingleton<INotificationQueue, NotificationQueue>();
            services.AddSingleton<INotificationChannel, NotificationChannel>();
            services.AddSingleton<INotificationPool, NotificationPool>();
            services.AddSingleton<IErrorHandler, ErrorHandler>();
            services.AddSingleton<IRateLimiter>(provider =>
            {
                var config = provider.GetRequiredService<NotificationConfiguration>();
                var logger = provider.GetService<ILogger<RateLimiter>>();
                return new RateLimiter(config, logger);
            });
            services.AddSingleton<IAnimationCache, AnimationCache>();
            services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
            services.AddSingleton<IConfigurationValidator, ConfigurationValidator>();
            services.AddSingleton<IConfigurationProvider, ConfigurationProvider>();
            services.AddSingleton<AnimationFactory>();
            
            // Регистрируем обработчики типов уведомлений
            services.AddSingleton<INotificationTypeHandlerRegistry, NotificationTypeHandlerRegistry>();
            services.AddSingleton<NotificationTypeHandlerRegistryService>();
            services.AddSingleton<INotificationTypeHandler, Handlers.CompactNotificationHandler>();
            services.AddSingleton<INotificationTypeHandler, Handlers.MusicNotificationHandler>();
            services.AddSingleton<INotificationTypeHandler, Handlers.CallNotificationHandler>();
            services.AddSingleton<INotificationTypeHandler, Handlers.StandardNotificationHandler>();

            // Регистрируем менеджеры
            services.AddSingleton<INotificationLifecycleManager, NotificationLifecycleManager>();
            services.AddSingleton<INotificationDisplayManager, NotificationDisplayManager>();
            services.AddSingleton<INotificationService, NotificationManager>();

            return services;
        }

        /// <summary>
        /// Регистрирует логирование
        /// </summary>
        public static IServiceCollection AddNotificationLogging(this IServiceCollection services, Microsoft.Extensions.Logging.LogLevel logLevel = Microsoft.Extensions.Logging.LogLevel.Information)
        {
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(logLevel);
            });

            return services;
        }

        /// <summary>
        /// Регистрирует все сервисы с конфигурацией по умолчанию
        /// </summary>
        public static IServiceCollection AddDynaNotyServices(this IServiceCollection services, Microsoft.Extensions.Logging.LogLevel logLevel = Microsoft.Extensions.Logging.LogLevel.Information)
        {
            return services
                .AddNotificationLogging(logLevel)
                .AddNotificationServices();
        }
    }
}
