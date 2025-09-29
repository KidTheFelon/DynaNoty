using System;
using System.Collections.Concurrent;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для мониторинга производительности
    /// </summary>
    public interface IPerformanceMonitor : IDisposable
    {
        /// <summary>
        /// Начинает измерение времени выполнения операции
        /// </summary>
        IDisposable StartTiming(string operationName);

        /// <summary>
        /// Увеличивает счетчик
        /// </summary>
        void IncrementCounter(string counterName, long value = 1);

        /// <summary>
        /// Устанавливает значение счетчика
        /// </summary>
        void SetCounter(string counterName, long value);

        /// <summary>
        /// Получает значение счетчика
        /// </summary>
        long GetCounter(string counterName);

        /// <summary>
        /// Получает все метрики
        /// </summary>
        ConcurrentDictionary<string, long> GetAllMetrics();

        /// <summary>
        /// Очищает все метрики
        /// </summary>
        void ClearMetrics();
    }
}
