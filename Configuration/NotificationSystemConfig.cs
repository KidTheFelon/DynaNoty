namespace DynaNoty.Configuration
{
    /// <summary>
    /// Конфигурация системных уведомлений
    /// </summary>
    public class NotificationSystemConfig
    {
        // === Системные уведомления ===
        /// <summary>
        /// Включить системные уведомления Windows
        /// </summary>
        public bool EnableSystemNotifications { get; set; } = true;

        /// <summary>
        /// Показывать системные уведомления только при свернутом приложении
        /// </summary>
        public bool SystemNotificationsOnlyWhenMinimized { get; set; } = false;

        /// <summary>
        /// Показывать системные уведомления для всех типов уведомлений
        /// </summary>
        public bool SystemNotificationsForAllTypes { get; set; } = true;

        /// <summary>
        /// Показывать системные уведомления только для важных уведомлений
        /// </summary>
        public bool SystemNotificationsForImportantOnly { get; set; } = false;

        /// <summary>
        /// Время отображения системного уведомления (секунды)
        /// </summary>
        public int SystemNotificationDuration { get; set; } = 5;

        /// <summary>
        /// Включить звук для системных уведомлений
        /// </summary>
        public bool EnableSystemNotificationSound { get; set; } = true;

        /// <summary>
        /// Включить действия в системных уведомлениях
        /// </summary>
        public bool EnableSystemNotificationActions { get; set; } = true;
    }
}
