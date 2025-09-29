using System;
using System.Collections.Generic;
using DynaNoty.Constants;

namespace DynaNoty.Models
{
    /// <summary>
    /// Модель данных уведомления
    /// </summary>
    public class NotificationData
    {
        public string Id { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Icon { get; }
        public NotificationType Type { get; }
        public DateTime CreatedAt { get; }
        public int RetryCount { get; private set; } = 0;
        public List<NotificationAction> Actions { get; }

        public NotificationData(string title, string subtitle, string icon = null, NotificationType type = NotificationType.Standard, List<NotificationAction> actions = null)
        {
            Id = Guid.NewGuid().ToString();
            Title = title;
            Subtitle = subtitle;
            Icon = icon ?? NotificationConstants.DEFAULT_ICON;
            Type = type;
            CreatedAt = DateTime.UtcNow;
            Actions = actions ?? new List<NotificationAction>();
        }

        public static NotificationData CreateMusicNotification(string title, string subtitle, string artist)
        {
            return new NotificationData(title, $"{subtitle} - {artist}", NotificationConstants.MUSIC_ICON, NotificationType.Music);
        }

        public static NotificationData CreateCallNotification(string title, string caller, string icon = null)
        {
            return new NotificationData(title, caller, icon ?? NotificationConstants.CALL_ICON, NotificationType.Call);
        }

        public static NotificationData CreateCompactNotification(string icon = null)
        {
            return new NotificationData(string.Empty, string.Empty, icon ?? NotificationConstants.DEFAULT_ICON, NotificationType.Compact);
        }

        /// <summary>
        /// Увеличивает счетчик попыток повторной обработки
        /// </summary>
        public void IncrementRetryCount()
        {
            RetryCount++;
        }
    }

    /// <summary>
    /// Типы уведомлений
    /// </summary>
    public enum NotificationType
    {
        Standard,
        Music,
        Call,
        Compact
    }
}
