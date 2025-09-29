using DynaNoty.Interfaces;
using DynaNoty.Models;

namespace DynaNoty.Services.Handlers
{
    /// <summary>
    /// Обработчик для уведомлений о звонках
    /// </summary>
    public class CallNotificationHandler : INotificationTypeHandler
    {
        public int Priority => 100;

        public bool CanHandle(NotificationType type)
        {
            return type == NotificationType.Call;
        }

        public void ShowNotification(DynamicIslandNotification notification, NotificationData data)
        {
            notification.ShowNotification(data.Title, data.Subtitle, data.Icon, true, data.Actions);
        }
    }
}
