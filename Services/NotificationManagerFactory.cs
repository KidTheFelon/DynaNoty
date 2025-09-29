using System;
using DynaNoty;
using DynaNoty.Interfaces;
using DynaNoty.Configuration;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Фабрика для создания NotificationManager с упрощенным API
    /// </summary>
    public static class NotificationManagerFactory
    {
        /// <summary>
        /// Создает NotificationManager с конфигурацией по умолчанию
        /// </summary>
        public static NotificationManager CreateDefault(ILoggerFactory loggerFactory = null)
        {
            var config = new NotificationConfiguration();
            return CreateWithConfig(config, loggerFactory);
        }

        /// <summary>
        /// Создает NotificationManager с пользовательской конфигурацией
        /// </summary>
        public static NotificationManager CreateWithConfig(NotificationConfiguration config, ILoggerFactory loggerFactory = null)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // Создаем все зависимости
            var dependencies = CreateDependencies(config, loggerFactory);
            
            // Создаем и возвращаем NotificationManager
            return new NotificationManager(dependencies);
        }

        /// <summary>
        /// Создает NotificationManager с минимальными зависимостями (для тестов)
        /// </summary>
        public static NotificationManager CreateMinimal(
            INotificationWindow window = null,
            NotificationConfiguration config = null,
            ILoggerFactory loggerFactory = null)
        {
            config ??= new NotificationConfiguration();
            
            var dependencies = new NotificationManagerDependencies
            {
                NotificationWindow = window ?? new NotificationWindow(config),
                Config = config,
                LoggerFactory = loggerFactory
            };

            return new NotificationManager(dependencies);
        }

        /// <summary>
        /// Создает все зависимости для NotificationManager
        /// </summary>
        private static NotificationManagerDependencies CreateDependencies(
            NotificationConfiguration config, 
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory?.CreateLogger<NotificationManager>();
            
            return new NotificationManagerDependencies
            {
                NotificationWindow = new NotificationWindow(config),
                Config = config,
                Queue = new NotificationQueue(loggerFactory?.CreateLogger<NotificationQueue>()),
                DisplayManager = CreateDisplayManager(config, loggerFactory),
                LifecycleManager = CreateLifecycleManager(config, loggerFactory),
                ErrorHandler = new ErrorHandler(loggerFactory?.CreateLogger<ErrorHandler>()),
                RateLimiter = new RateLimiter(
                    config,
                    loggerFactory?.CreateLogger<RateLimiter>()),
                PerformanceMonitor = new PerformanceMonitor(loggerFactory?.CreateLogger<PerformanceMonitor>()),
                Logger = logger,
                LoggerFactory = loggerFactory
            };
        }

        /// <summary>
        /// Создает NotificationDisplayManager с зависимостями
        /// </summary>
        private static INotificationDisplayManager CreateDisplayManager(
            NotificationConfiguration config,
            ILoggerFactory loggerFactory)
        {
            var validationService = new InputValidationService(
                config,
                loggerFactory?.CreateLogger<InputValidationService>());
            
            var pool = new NotificationPool(config,
                new SystemThemeService(loggerFactory?.CreateLogger<SystemThemeService>()),
                loggerFactory?.CreateLogger<NotificationPool>());
            
            var positioningService = new NotificationPositioningService(config,
                loggerFactory?.CreateLogger<NotificationPositioningService>());
            
            var lifecycleManager = new NotificationLifecycleManager(
                new NotificationWindow(config),
                positioningService,
                pool,
                loggerFactory?.CreateLogger<NotificationLifecycleManager>());
            
            var registry = new NotificationTypeHandlerRegistry(
                loggerFactory?.CreateLogger<NotificationTypeHandlerRegistry>());
            
            // Регистрируем обработчики
            registry.RegisterHandler(new Handlers.CompactNotificationHandler());
            registry.RegisterHandler(new Handlers.MusicNotificationHandler());
            registry.RegisterHandler(new Handlers.CallNotificationHandler());
            registry.RegisterHandler(new Handlers.StandardNotificationHandler());

            return new NotificationDisplayManager(
                pool,
                lifecycleManager,
                validationService,
                registry,
                loggerFactory?.CreateLogger<NotificationDisplayManager>());
        }

        /// <summary>
        /// Создает NotificationLifecycleManager с зависимостями
        /// </summary>
        private static INotificationLifecycleManager CreateLifecycleManager(
            NotificationConfiguration config,
            ILoggerFactory loggerFactory)
        {
            var positioningService = new NotificationPositioningService(config,
                loggerFactory?.CreateLogger<NotificationPositioningService>());
            
            var pool = new NotificationPool(config,
                new SystemThemeService(loggerFactory?.CreateLogger<SystemThemeService>()),
                loggerFactory?.CreateLogger<NotificationPool>());
            
            return new NotificationLifecycleManager(
                new NotificationWindow(config),
                positioningService,
                pool,
                loggerFactory?.CreateLogger<NotificationLifecycleManager>());
        }
    }
}