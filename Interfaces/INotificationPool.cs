using System;
using DynaNoty;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для пула объектов уведомлений
    /// </summary>
    public interface INotificationPool : IDisposable
    {
        /// <summary>
        /// Получает уведомление из пула или создает новое
        /// </summary>
        DynamicIslandNotification GetNotification();

        /// <summary>
        /// Возвращает уведомление в пул для переиспользования
        /// </summary>
        void ReturnNotification(DynamicIslandNotification notification);

        /// <summary>
        /// Очищает пул
        /// </summary>
        void Clear();

        /// <summary>
        /// Количество уведомлений в пуле
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Общее количество созданных уведомлений
        /// </summary>
        int TotalCreated { get; }

        /// <summary>
        /// Статистика производительности пула
        /// </summary>
        (int PoolSize, int TotalCreated, int ReuseRate) GetPerformanceStats();
    }
}
