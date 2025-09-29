using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Упрощенный обработчик ошибок для критических операций
    /// </summary>
    public static class SimpleErrorHandler
    {
        /// <summary>
        /// Выполняет операцию с простой обработкой ошибок
        /// </summary>
        public static void ExecuteSafe(Action operation, string operationName = null, ILogger logger = null)
        {
            try
            {
                operation?.Invoke();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Ошибка в операции {OperationName}", operationName ?? "Unknown");
                // Не пробрасываем исключение - graceful degradation
            }
        }

        /// <summary>
        /// Выполняет операцию с возвращаемым значением и простой обработкой ошибок
        /// </summary>
        public static T ExecuteSafe<T>(Func<T> operation, T defaultValue = default(T), string operationName = null, ILogger logger = null)
        {
            try
            {
                return operation != null ? operation() : defaultValue;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Ошибка в операции {OperationName}", operationName ?? "Unknown");
                return defaultValue;
            }
        }

        /// <summary>
        /// Выполняет асинхронную операцию с простой обработкой ошибок
        /// </summary>
        public static async Task ExecuteSafeAsync(Func<Task> operation, string operationName = null, ILogger logger = null)
        {
            try
            {
                if (operation != null)
                    await operation();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Ошибка в асинхронной операции {OperationName}", operationName ?? "Unknown");
                // Не пробрасываем исключение - graceful degradation
            }
        }

        /// <summary>
        /// Выполняет асинхронную операцию с возвращаемым значением и простой обработкой ошибок
        /// </summary>
        public static async Task<T> ExecuteSafeAsync<T>(Func<Task<T>> operation, T defaultValue = default(T), string operationName = null, ILogger logger = null)
        {
            try
            {
                return operation != null ? await operation() : defaultValue;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Ошибка в асинхронной операции {OperationName}", operationName ?? "Unknown");
                return defaultValue;
            }
        }

        /// <summary>
        /// Выполняет операцию с retry для критических операций
        /// </summary>
        public static bool ExecuteWithRetry(Action operation, int maxRetries = 3, int delayMs = 100, string operationName = null, ILogger logger = null)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    operation?.Invoke();
                    return true;
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Попытка {Attempt}/{MaxRetries} не удалась для операции {OperationName}", 
                        attempt, maxRetries, operationName ?? "Unknown");
                    
                    if (attempt < maxRetries)
                    {
                        System.Threading.Thread.Sleep(delayMs * attempt);
                    }
                }
            }
            
            logger?.LogError("Операция {OperationName} не удалась после {MaxRetries} попыток", 
                operationName ?? "Unknown", maxRetries);
            return false;
        }

        /// <summary>
        /// Выполняет операцию с retry и возвращаемым значением
        /// </summary>
        public static T ExecuteWithRetry<T>(Func<T> operation, T defaultValue = default(T), int maxRetries = 3, int delayMs = 100, string operationName = null, ILogger logger = null)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return operation != null ? operation() : defaultValue;
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Попытка {Attempt}/{MaxRetries} не удалась для операции {OperationName}", 
                        attempt, maxRetries, operationName ?? "Unknown");
                    
                    if (attempt < maxRetries)
                    {
                        System.Threading.Thread.Sleep(delayMs * attempt);
                    }
                }
            }
            
            logger?.LogError("Операция {OperationName} не удалась после {MaxRetries} попыток", 
                operationName ?? "Unknown", maxRetries);
            return defaultValue;
        }

        /// <summary>
        /// Логирует ошибку с контекстом
        /// </summary>
        public static void LogError(Exception exception, string message, ILogger logger = null, params object[] args)
        {
            if (logger != null)
            {
                logger.LogError(exception, message, args);
            }
            else
            {
                Console.WriteLine($"Ошибка: {message} - {exception?.Message}");
            }
        }

        /// <summary>
        /// Логирует предупреждение с контекстом
        /// </summary>
        public static void LogWarning(Exception exception, string message, ILogger logger = null, params object[] args)
        {
            if (logger != null)
            {
                logger.LogWarning(exception, message, args);
            }
            else
            {
                Console.WriteLine($"Предупреждение: {message} - {exception?.Message}");
            }
        }
    }

    /// <summary>
    /// Упрощенный обработчик ошибок для UI операций
    /// </summary>
    public static class UIErrorHandler
    {
        /// <summary>
        /// Выполняет UI операцию с обработкой ошибок
        /// </summary>
        public static void ExecuteUI(Action operation, string operationName = null, ILogger logger = null)
        {
            try
            {
                operation?.Invoke();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "UI ошибка в операции {OperationName}", operationName ?? "Unknown");
                
                // Можно добавить показ пользователю простого сообщения
                // MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Выполняет асинхронную UI операцию с обработкой ошибок
        /// </summary>
        public static async Task ExecuteUIAsync(Func<Task> operation, string operationName = null, ILogger logger = null)
        {
            try
            {
                if (operation != null)
                    await operation();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "UI ошибка в асинхронной операции {OperationName}", operationName ?? "Unknown");
                
                // Можно добавить показ пользователю простого сообщения
                // MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
