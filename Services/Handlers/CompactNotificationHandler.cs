using DynaNoty.Interfaces;
using DynaNoty.Models;

namespace DynaNoty.Services.Handlers
{
    /// <summary>
    /// Обработчик для компактных уведомлений
    /// </summary>
    public class CompactNotificationHandler : INotificationTypeHandler
    {
        public int Priority => 100;

        public bool CanHandle(NotificationType type)
        {
            return type == NotificationType.Compact;
        }

        public void ShowNotification(DynamicIslandNotification notification, NotificationData data)
        {
            notification.ShowCompact(data.Icon);
        }
    }
}
