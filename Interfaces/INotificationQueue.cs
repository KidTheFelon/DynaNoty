using DynaNoty.Models;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для очереди уведомлений
    /// </summary>
    public interface INotificationQueue
    {
        /// <summary>
        /// Количество уведомлений в очереди
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Добавляет уведомление в очередь
        /// </summary>
        void Enqueue(NotificationData notification);

        /// <summary>
        /// Извлекает следующее уведомление из очереди
        /// </summary>
        NotificationData Dequeue();

        /// <summary>
        /// Просматривает следующее уведомление без извлечения
        /// </summary>
        NotificationData Peek();

        /// <summary>
        /// Очищает очередь
        /// </summary>
        void Clear();

        /// <summary>
        /// Возвращает все уведомления в очереди
        /// </summary>
        NotificationData[] ToArray();
    }
}
