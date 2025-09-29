using DynaNoty.Models;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Обработчик для конкретного типа уведомления
    /// </summary>
    public interface INotificationTypeHandler
    {
        /// <summary>
        /// Показывает уведомление определенного типа
        /// </summary>
        void ShowNotification(DynamicIslandNotification notification, NotificationData data);

        /// <summary>
        /// Проверяет, может ли обработчик работать с данным типом уведомления
        /// </summary>
        bool CanHandle(NotificationType type);

        /// <summary>
        /// Приоритет обработчика (больше = выше приоритет)
        /// </summary>
        int Priority { get; }
    }
}
