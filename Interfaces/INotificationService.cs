using System;
using System.Collections.Generic;
using DynaNoty.Events;
using DynaNoty.Models;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Сервис для управления уведомлениями Dynamic Island
    /// </summary>
    public interface INotificationService : IDisposable
    {
        /// <summary>
        /// Показать обычное уведомление
        /// </summary>
        void ShowNotification(string title, string subtitle, string icon = "🔔", List<NotificationAction> actions = null);

        /// <summary>
        /// Показать музыкальное уведомление
        /// </summary>
        void ShowMusicNotification(string title, string subtitle, string artist);

        /// <summary>
        /// Показать уведомление о звонке
        /// </summary>
        void ShowCallNotification(string title, string caller, string icon = "📞");

        /// <summary>
        /// Показать компактное уведомление
        /// </summary>
        void ShowCompactNotification(string icon = "🔔");

        /// <summary>
        /// Закрыть все активные уведомления
        /// </summary>
        void ClearAllNotifications();

        /// <summary>
        /// Количество активных уведомлений
        /// </summary>
        int ActiveNotificationCount { get; }

        /// <summary>
        /// Событие закрытия уведомления
        /// </summary>
        event EventHandler<NotificationDismissedEventArgs> NotificationDismissed;

        /// <summary>
        /// Событие нажатия на действие уведомления
        /// </summary>
        event EventHandler<NotificationActionEventArgs> NotificationActionClicked;
    }
}
