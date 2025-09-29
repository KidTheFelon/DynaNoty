using System;
using System.Collections.Generic;
using System.Linq;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Реализация реестра обработчиков типов уведомлений
    /// </summary>
    public class NotificationTypeHandlerRegistry : INotificationTypeHandlerRegistry
    {
        private readonly Dictionary<NotificationType, INotificationTypeHandler> _handlers;
        private readonly ILogger<NotificationTypeHandlerRegistry> _logger;

        public NotificationTypeHandlerRegistry(ILogger<NotificationTypeHandlerRegistry> logger = null)
        {
            _handlers = new Dictionary<NotificationType, INotificationTypeHandler>();
            _logger = logger;
        }

        public INotificationTypeHandler GetHandler(NotificationType type)
        {
            if (_handlers.TryGetValue(type, out var handler))
            {
                _logger?.LogDebug("Найден обработчик для типа {Type}", type);
                return handler;
            }

            _logger?.LogWarning("Обработчик для типа {Type} не найден, используется обработчик по умолчанию", type);

            // Возвращаем обработчик по умолчанию (Standard)
            return _handlers.TryGetValue(NotificationType.Standard, out var defaultHandler)
                ? defaultHandler
                : throw new InvalidOperationException($"Обработчик для типа {type} не найден и нет обработчика по умолчанию");
        }

        public void RegisterHandler(INotificationTypeHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var supportedTypes = Enum.GetValues<NotificationType>()
                .Where(type => handler.CanHandle(type))
                .ToList();

            foreach (var type in supportedTypes)
            {
                if (_handlers.ContainsKey(type))
                {
                    var existingHandler = _handlers[type];
                    if (handler.Priority > existingHandler.Priority)
                    {
                        _handlers[type] = handler;
                        _logger?.LogDebug("Обработчик для типа {Type} заменен на новый с приоритетом {Priority}",
                            type, handler.Priority);
                    }
                    else
                    {
                        _logger?.LogDebug("Обработчик для типа {Type} не заменен, приоритет {Priority} <= {ExistingPriority}",
                            type, handler.Priority, existingHandler.Priority);
                    }
                }
                else
                {
                    _handlers[type] = handler;
                    _logger?.LogDebug("Зарегистрирован обработчик для типа {Type} с приоритетом {Priority}",
                        type, handler.Priority);
                }
            }
        }

        public System.Collections.Generic.IEnumerable<INotificationTypeHandler> GetAllHandlers()
        {
            return _handlers.Values.Distinct().OrderByDescending(h => h.Priority);
        }

        public void Dispose()
        {
            _handlers.Clear();
            _logger?.LogInformation("NotificationTypeHandlerRegistry освобожден");
        }
    }
}
