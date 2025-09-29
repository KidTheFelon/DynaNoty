using System;
using System.Threading.Tasks;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для централизованной обработки ошибок
    /// </summary>
    public interface IErrorHandler : IDisposable
    {
        /// <summary>
        /// Выполняет операцию с retry логикой
        /// </summary>
        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName = null);

        /// <summary>
        /// Выполняет операцию с retry логикой (синхронная версия)
        /// </summary>
        T ExecuteWithRetry<T>(Func<T> operation, string operationName = null);

        /// <summary>
        /// Выполняет операцию без возвращаемого значения с retry логикой
        /// </summary>
        Task ExecuteWithRetryAsync(Func<Task> operation, string operationName = null);

        /// <summary>
        /// Выполняет операцию без возвращаемого значения с retry логикой (синхронная версия)
        /// </summary>
        void ExecuteWithRetry(Action operation, string operationName = null);

        /// <summary>
        /// Логирует ошибку с контекстом
        /// </summary>
        void LogError(Exception exception, string message, params object[] args);

        /// <summary>
        /// Логирует предупреждение с контекстом
        /// </summary>
        void LogWarning(Exception exception, string message, params object[] args);
    }
}
