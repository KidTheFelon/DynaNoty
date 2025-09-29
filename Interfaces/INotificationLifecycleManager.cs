using System;
using DynaNoty.Events;
using DynaNoty.Models;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для управления жизненным циклом уведомлений
    /// </summary>
    public interface INotificationLifecycleManager : IDisposable
    {
        /// <summary>
        /// Количество активных уведомлений
        /// </summary>
        int ActiveNotificationCount { get; }

        /// <summary>
        /// Событие закрытия уведомления
        /// </summary>
        event EventHandler<NotificationDismissedEventArgs> NotificationDismissed;

        /// <summary>
        /// Событие нажатия действия уведомления
        /// </summary>
        event EventHandler<NotificationActionEventArgs> NotificationActionClicked;

        /// <summary>
        /// Добавляет уведомление в активный список
        /// </summary>
        void AddNotification(DynamicIslandNotification notification, int maxNotifications);

        /// <summary>
        /// Удаляет уведомление из активного списка
        /// </summary>
        void RemoveNotification(DynamicIslandNotification notification);

        /// <summary>
        /// Очищает все активные уведомления
        /// </summary>
        void ClearAllNotifications();

        /// <summary>
        /// Очищает завершенные уведомления
        /// </summary>
        void CleanupCompletedNotifications();

        /// <summary>
        /// Получает пул уведомлений
        /// </summary>
        INotificationPool GetPool();
    }
}
