using System;
using System.Threading;
using System.Threading.Tasks;
using DynaNoty.Models;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для асинхронного канала уведомлений
    /// </summary>
    public interface INotificationChannel : IDisposable
    {
        /// <summary>
        /// Добавляет уведомление в канал
        /// </summary>
        Task<bool> WriteAsync(NotificationData notification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Читает уведомление из канала
        /// </summary>
        Task<NotificationData> ReadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Пытается прочитать уведомление без блокировки
        /// </summary>
        bool TryRead(out NotificationData notification);

        /// <summary>
        /// Ожидает завершения канала
        /// </summary>
        Task WaitToReadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Завершает канал
        /// </summary>
        void Complete();

        /// <summary>
        /// Завершает канал с ошибкой
        /// </summary>
        void Complete(Exception exception);
    }
}
