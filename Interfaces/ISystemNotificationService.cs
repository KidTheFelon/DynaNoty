using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynaNoty.Models;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с системными уведомлениями Windows
    /// </summary>
    public interface ISystemNotificationService : IDisposable
    {
        /// <summary>
        /// Показывает системное уведомление
        /// </summary>
        /// <param name="title">Заголовок уведомления</param>
        /// <param name="body">Текст уведомления</param>
        /// <param name="icon">Иконка уведомления</param>
        /// <param name="actions">Действия уведомления</param>
        /// <returns>Идентификатор уведомления</returns>
        Task<string> ShowNotificationAsync(string title, string body, string icon = null, List<NotificationAction> actions = null);

        /// <summary>
        /// Показывает системное уведомление с данными
        /// </summary>
        /// <param name="notificationData">Данные уведомления</param>
        /// <returns>Идентификатор уведомления</returns>
        Task<string> ShowNotificationAsync(NotificationData notificationData);

        /// <summary>
        /// Скрывает уведомление по идентификатору
        /// </summary>
        /// <param name="notificationId">Идентификатор уведомления</param>
        Task HideNotificationAsync(string notificationId);

        /// <summary>
        /// Скрывает все уведомления приложения
        /// </summary>
        Task HideAllNotificationsAsync();

        /// <summary>
        /// Проверяет, поддерживаются ли системные уведомления
        /// </summary>
        /// <returns>True, если поддерживаются</returns>
        bool IsSupported();

        /// <summary>
        /// Запрашивает разрешение на показ уведомлений
        /// </summary>
        /// <returns>True, если разрешение получено</returns>
        Task<bool> RequestPermissionAsync();

        /// <summary>
        /// Проверяет, есть ли разрешение на показ уведомлений
        /// </summary>
        /// <returns>True, если разрешение есть</returns>
        bool HasPermission();

        /// <summary>
        /// Событие клика по уведомлению
        /// </summary>
        event EventHandler<SystemNotificationClickedEventArgs> NotificationClicked;

        /// <summary>
        /// Событие клика по действию уведомления
        /// </summary>
        event EventHandler<SystemNotificationActionClickedEventArgs> ActionClicked;

        /// <summary>
        /// Событие отклонения уведомления
        /// </summary>
        event EventHandler<SystemNotificationDismissedEventArgs> NotificationDismissed;
    }

    /// <summary>
    /// Аргументы события клика по системному уведомлению
    /// </summary>
    public class SystemNotificationClickedEventArgs : EventArgs
    {
        public string NotificationId { get; }
        public string Title { get; }
        public string Body { get; }

        public SystemNotificationClickedEventArgs(string notificationId, string title, string body)
        {
            NotificationId = notificationId;
            Title = title;
            Body = body;
        }
    }

    /// <summary>
    /// Аргументы события клика по действию системного уведомления
    /// </summary>
    public class SystemNotificationActionClickedEventArgs : EventArgs
    {
        public string NotificationId { get; }
        public string ActionId { get; }
        public string ActionTitle { get; }

        public SystemNotificationActionClickedEventArgs(string notificationId, string actionId, string actionTitle)
        {
            NotificationId = notificationId;
            ActionId = actionId;
            ActionTitle = actionTitle;
        }
    }

    /// <summary>
    /// Аргументы события отклонения системного уведомления
    /// </summary>
    public class SystemNotificationDismissedEventArgs : EventArgs
    {
        public string NotificationId { get; }
        public string Title { get; }
        public string Body { get; }

        public SystemNotificationDismissedEventArgs(string notificationId, string title, string body)
        {
            NotificationId = notificationId;
            Title = title;
            Body = body;
        }
    }
}
