using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;
using DynaNoty.Configuration;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для ограничения частоты уведомлений
    /// </summary>
    public class RateLimiter : IRateLimiter
    {
        private readonly ILogger<RateLimiter> _logger;
        private readonly Dictionary<string, Queue<DateTime>> _requestHistory = new();
        private readonly object _lock = new object();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;
        private bool _disposed = false;

        public RateLimiter(NotificationConfiguration config, ILogger<RateLimiter> logger = null)
        {
            _maxRequests = config?.Advanced?.RateLimit ?? 20;
            _timeWindow = TimeSpan.FromMinutes(config?.Advanced?.RateLimitWindowMinutes ?? 1);
            _logger = logger;
        }

        // Приватный конструктор для внутреннего использования
        private RateLimiter(int maxRequests, TimeSpan timeWindow, ILogger<RateLimiter> logger)
        {
            _maxRequests = maxRequests;
            _timeWindow = timeWindow;
            _logger = logger;
        }

        /// <summary>
        /// Проверяет, можно ли выполнить запрос
        /// </summary>
        public bool CanMakeRequest(string key = "default")
        {
            if (_disposed) return false;

            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var cutoff = now - _timeWindow;

                // Получаем или создаем историю для ключа
                if (!_requestHistory.TryGetValue(key, out var history))
                {
                    history = new Queue<DateTime>();
                    _requestHistory[key] = history;
                }

                // Удаляем старые записи
                while (history.Count > 0 && history.Peek() < cutoff)
                {
                    history.Dequeue();
                }

                // Проверяем лимит
                if (history.Count >= _maxRequests)
                {
                    _logger?.LogWarning("Rate limit превышен для ключа {Key}. Запросов: {Count}/{MaxRequests}", 
                        key, history.Count, _maxRequests);
                    return false;
                }

                // Добавляем текущий запрос
                history.Enqueue(now);
                
                _logger?.LogDebug("Запрос разрешен для ключа {Key}. Запросов: {Count}/{MaxRequests}", 
                    key, history.Count, _maxRequests);
                
                return true;
            }
        }

        /// <summary>
        /// Получает время до следующего разрешенного запроса
        /// </summary>
        public TimeSpan? GetTimeUntilNextRequest(string key = "default")
        {
            if (_disposed) return null;

            lock (_lock)
            {
                if (!_requestHistory.TryGetValue(key, out var history) || history.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                var oldestRequest = history.Peek();
                var timeUntilOldestExpires = (oldestRequest + _timeWindow) - DateTime.UtcNow;
                
                return timeUntilOldestExpires > TimeSpan.Zero ? timeUntilOldestExpires : TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Очищает историю для ключа
        /// </summary>
        public void ClearHistory(string key = "default")
        {
            if (_disposed) return;

            lock (_lock)
            {
                if (_requestHistory.TryGetValue(key, out var history))
                {
                    history.Clear();
                    _logger?.LogDebug("История очищена для ключа {Key}", key);
                }
            }
        }

        /// <summary>
        /// Очищает всю историю
        /// </summary>
        public void ClearAllHistory()
        {
            if (_disposed) return;

            lock (_lock)
            {
                _requestHistory.Clear();
                _logger?.LogDebug("Вся история rate limiter очищена");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ClearAllHistory();
                _logger?.LogDebug("RateLimiter освобожден");
            }
        }
    }
}
