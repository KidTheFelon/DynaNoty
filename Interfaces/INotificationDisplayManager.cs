using System;
using System.Threading.Tasks;
using DynaNoty.Events;
using DynaNoty.Models;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для отображения уведомлений
    /// </summary>
    public interface INotificationDisplayManager : IDisposable
    {
        /// <summary>
        /// Событие закрытия уведомления
        /// </summary>
        event EventHandler<NotificationDismissedEventArgs> NotificationDismissed;

        /// <summary>
        /// Событие нажатия действия уведомления
        /// </summary>
        event EventHandler<NotificationActionEventArgs> NotificationActionClicked;

        /// <summary>
        /// Показывает уведомление
        /// </summary>
        Task ShowNotificationAsync(Models.NotificationData notificationData, int maxNotifications);
    }
}
