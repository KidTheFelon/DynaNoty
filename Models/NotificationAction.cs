using System;

namespace DynaNoty.Models
{
    /// <summary>
    /// Действие уведомления
    /// </summary>
    public class NotificationAction
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public object Data { get; set; }
        public string Icon { get; set; } = "⚡";

        public NotificationAction(string id, string text, object data = null, string icon = "⚡")
        {
            Id = id;
            Text = text;
            Data = data;
            Icon = icon;
        }
    }

    /// <summary>
    /// Аргументы события действия уведомления
    /// </summary>
    public class NotificationActionEventArgs : EventArgs
    {
        public string ActionId { get; }
        public string ActionText { get; }
        public object Data { get; }

        public NotificationActionEventArgs(string actionId, string actionText, object data = null)
        {
            ActionId = actionId;
            ActionText = actionText;
            Data = data;
        }
    }
}
