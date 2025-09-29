using System;
using DynaNoty.Interfaces;
using DynaNoty.Models;

namespace DynaNoty.Services.Handlers
{
    /// <summary>
    /// Обработчик для стандартных уведомлений (по умолчанию)
    /// </summary>
    public class StandardNotificationHandler : INotificationTypeHandler
    {
        public int Priority => 10; // Низкий приоритет для обработчика по умолчанию

        public bool CanHandle(NotificationType type)
        {
            return type == NotificationType.Standard;
        }

        public void ShowNotification(DynamicIslandNotification notification, NotificationData data)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Валидация данных
            var title = string.IsNullOrWhiteSpace(data.Title) ? "Уведомление" : data.Title.Trim();
            var subtitle = data.Subtitle?.Trim() ?? string.Empty;
            var icon = string.IsNullOrWhiteSpace(data.Icon) ? "🔔" : data.Icon.Trim();

            System.Diagnostics.Debug.WriteLine($"StandardNotificationHandler.ShowNotification вызван: {title} - {subtitle}");
            notification.ShowNotification(title, subtitle, icon, true, data.Actions);
            System.Diagnostics.Debug.WriteLine("StandardNotificationHandler.ShowNotification завершен");
        }
    }
}
