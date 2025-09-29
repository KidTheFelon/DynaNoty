using DynaNoty.Interfaces;
using DynaNoty.Models;

namespace DynaNoty.Services.Handlers
{
    /// <summary>
    /// Обработчик для музыкальных уведомлений
    /// </summary>
    public class MusicNotificationHandler : INotificationTypeHandler
    {
        public int Priority => 100;

        public bool CanHandle(NotificationType type)
        {
            return type == NotificationType.Music;
        }

        public void ShowNotification(DynamicIslandNotification notification, NotificationData data)
        {
            notification.ShowNotification(data.Title, data.Subtitle, data.Icon, true, data.Actions);
        }
    }
}
