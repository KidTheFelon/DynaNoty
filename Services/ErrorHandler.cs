using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;

namespace DynaNoty.Services
{
    /// <summary>
    /// Централизованный обработчик ошибок с retry логикой
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger<ErrorHandler> _logger;
        private readonly int _maxRetries;
        private readonly TimeSpan _baseDelay;
        private bool _disposed = false;

        public ErrorHandler(ILogger<ErrorHandler> logger = null, int maxRetries = 3, TimeSpan? baseDelay = null)
        {
            _logger = logger;
            _maxRetries = maxRetries;
            _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(100);
        }

        /// <summary>
        /// Выполняет операцию с retry логикой
        /// </summary>
        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName = null)
        {
            var lastException = (Exception)null;

            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger?.LogWarning(ex, "Ошибка в операции {OperationName} (попытка {Attempt}/{MaxRetries})",
                        operationName ?? "Unknown", attempt, _maxRetries);

                    if (attempt < _maxRetries)
                    {
                        var delay = CalculateDelay(attempt);
                        await Task.Delay(delay);
                    }
                }
            }

            _logger?.LogError(lastException, "Операция {OperationName} не удалась после {MaxRetries} попыток",
                operationName ?? "Unknown", _maxRetries);
            throw lastException;
        }

        /// <summary>
        /// Выполняет операцию с retry логикой (синхронная версия)
        /// </summary>
        public T ExecuteWithRetry<T>(Func<T> operation, string operationName = null)
        {
            var lastException = (Exception)null;

            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger?.LogWarning(ex, "Ошибка в операции {OperationName} (попытка {Attempt}/{MaxRetries})",
                        operationName ?? "Unknown", attempt, _maxRetries);

                    if (attempt < _maxRetries)
                    {
                        var delay = CalculateDelay(attempt);
                        Task.Delay(delay).Wait();
                    }
                }
            }

            _logger?.LogError(lastException, "Операция {OperationName} не удалась после {MaxRetries} попыток",
                operationName ?? "Unknown", _maxRetries);
            throw lastException;
        }

        /// <summary>
        /// Выполняет операцию без возвращаемого значения с retry логикой
        /// </summary>
        public async Task ExecuteWithRetryAsync(Func<Task> operation, string operationName = null)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await operation();
                return true;
            }, operationName);
        }

        /// <summary>
        /// Выполняет операцию без возвращаемого значения с retry логикой (синхронная версия)
        /// </summary>
        public void ExecuteWithRetry(Action operation, string operationName = null)
        {
            ExecuteWithRetry(() =>
            {
                operation();
                return true;
            }, operationName);
        }

        /// <summary>
        /// Вычисляет задержку для следующей попытки (экспоненциальная задержка)
        /// </summary>
        private TimeSpan CalculateDelay(int attempt)
        {
            var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
            return TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds, 5000)); // Максимум 5 секунд
        }

        /// <summary>
        /// Логирует ошибку с контекстом
        /// </summary>
        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger?.LogError(exception, message, args);
        }

        /// <summary>
        /// Логирует предупреждение с контекстом
        /// </summary>
        public void LogWarning(Exception exception, string message, params object[] args)
        {
            _logger?.LogWarning(exception, message, args);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _logger?.LogDebug("ErrorHandler освобожден");
            }
        }
    }
}
