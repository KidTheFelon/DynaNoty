using System;
using System.Collections.Generic;
using System.Linq;
using DynaNoty.Models;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;

namespace DynaNoty.Services
{
    /// <summary>
    /// Очередь уведомлений с приоритетами
    /// </summary>
    public class NotificationQueue : INotificationQueue
    {
        private readonly Queue<NotificationData> _queue = new();
        private readonly ILogger<NotificationQueue> _logger;
        private readonly object _lock = new object();

        public NotificationQueue(ILogger<NotificationQueue> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Количество уведомлений в очереди
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary>
        /// Добавляет уведомление в очередь
        /// </summary>
        public void Enqueue(NotificationData notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            lock (_lock)
            {
                _queue.Enqueue(notification);
                _logger?.LogDebug("Уведомление добавлено в очередь. Размер очереди: {Count}", _queue.Count);
            }
        }

        /// <summary>
        /// Извлекает следующее уведомление из очереди
        /// </summary>
        public NotificationData Dequeue()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _logger?.LogDebug("Попытка извлечения из пустой очереди");
                    return null;
                }

                try
                {
                    var notification = _queue.Dequeue();

                    // Проверяем на null сразу после извлечения
                    if (notification == null)
                    {
                        _logger?.LogWarning("Очередь вернула null уведомление");
                        return null;
                    }

                    _logger?.LogDebug("Уведомление {Title} извлечено из очереди. Осталось: {Count}",
                        notification.Title, _queue.Count);
                    return notification;
                }
                catch (InvalidOperationException ex)
                {
                    _logger?.LogError(ex, "Ошибка извлечения из очереди - очередь была изменена во время операции");
                    return null;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Неожиданная ошибка при извлечении из очереди");
                    return null;
                }
            }
        }

        /// <summary>
        /// Просматривает следующее уведомление без извлечения
        /// </summary>
        public NotificationData Peek()
        {
            lock (_lock)
            {
                return _queue.Count > 0 ? _queue.Peek() : null;
            }
        }

        /// <summary>
        /// Очищает очередь
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                var count = _queue.Count;
                _queue.Clear();
                _logger?.LogInformation("Очередь очищена. Удалено уведомлений: {Count}", count);
            }
        }

        /// <summary>
        /// Возвращает все уведомления в очереди
        /// </summary>
        public NotificationData[] ToArray()
        {
            lock (_lock)
            {
                return _queue.ToArray();
            }
        }
    }
}
