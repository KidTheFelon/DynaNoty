using System;
using System.Collections.Generic;
using System.Linq;
using DynaNoty.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для автоматической регистрации обработчиков в реестр
    /// </summary>
    public class NotificationTypeHandlerRegistryService
    {
        private readonly INotificationTypeHandlerRegistry _registry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationTypeHandlerRegistryService> _logger;

        public NotificationTypeHandlerRegistryService(
            INotificationTypeHandlerRegistry registry,
            IServiceProvider serviceProvider,
            ILogger<NotificationTypeHandlerRegistryService> logger = null)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger;
        }

        /// <summary>
        /// Регистрирует все обработчики из DI контейнера
        /// </summary>
        public void RegisterAllHandlers()
        {
            var handlers = _serviceProvider.GetServices<INotificationTypeHandler>();
            
            foreach (var handler in handlers)
            {
                _registry.RegisterHandler(handler);
                _logger?.LogDebug("Зарегистрирован обработчик {HandlerType}", handler.GetType().Name);
            }

            _logger?.LogInformation("Зарегистрировано {Count} обработчиков типов уведомлений", handlers.Count());
        }
    }
}
