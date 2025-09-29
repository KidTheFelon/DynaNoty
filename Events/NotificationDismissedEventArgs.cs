using System;

namespace DynaNoty.Events
{
    /// <summary>
    /// Аргументы события закрытия уведомления
    /// </summary>
    public class NotificationDismissedEventArgs : EventArgs
    {
        public string NotificationId { get; }
        public DateTime DismissedAt { get; }

        public NotificationDismissedEventArgs(string notificationId)
        {
            NotificationId = notificationId;
            DismissedAt = DateTime.UtcNow;
        }
    }
}
