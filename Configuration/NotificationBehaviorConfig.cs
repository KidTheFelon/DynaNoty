namespace DynaNoty.Configuration
{
    /// <summary>
    /// Конфигурация поведения уведомлений
    /// </summary>
    public class NotificationBehaviorConfig
    {
        // === Основные настройки ===
        /// <summary>
        /// Время автоматического скрытия уведомлений (секунды)
        /// </summary>
        public int AutoHideTimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Интервал очистки завершенных уведомлений (секунды)
        /// </summary>
        public int CleanupIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// Максимальное количество уведомлений одновременно
        /// </summary>
        public int MaxNotifications { get; set; } = 3;

        // === Анимации ===
        /// <summary>
        /// Включить анимации
        /// </summary>
        public bool EnableAnimations { get; set; } = true;

        /// <summary>
        /// Длительность анимации появления (миллисекунды)
        /// </summary>
        public int AppearAnimationDuration { get; set; } = 300;

        /// <summary>
        /// Длительность анимации расширения (миллисекунды)
        /// </summary>
        public int ExpandAnimationDuration { get; set; } = 400;

        /// <summary>
        /// Длительность анимации сдвига (миллисекунды)
        /// </summary>
        public int RepositionAnimationDuration { get; set; } = 300;

        /// <summary>
        /// Длительность анимации закрытия (миллисекунды)
        /// </summary>
        public int DismissAnimationDuration { get; set; } = 500;

        /// <summary>
        /// Расстояние сдвига при закрытии (пиксели)
        /// </summary>
        public double DismissSlideDistance { get; set; } = 50;

        /// <summary>
        /// Задержка перед расширением (миллисекунды)
        /// </summary>
        public int ExpandDelay { get; set; } = 1000;

        /// <summary>
        /// Время отображения свернутого уведомления (миллисекунды)
        /// </summary>
        public int CompactDisplayDuration { get; set; } = 3000;

        /// <summary>
        /// Время отображения расширенного уведомления (миллисекунды)
        /// </summary>
        public int ExpandedDisplayDuration { get; set; } = 5000;

        /// <summary>
        /// Время отображения полностью раскрытого уведомления (миллисекунды)
        /// </summary>
        public int FullyExpandedDisplayDuration { get; set; } = 8000;

        /// <summary>
        /// Включить автораскрытие уведомлений
        /// </summary>
        public bool EnableAutoExpand { get; set; } = true;

        // === Поведение ===
        /// <summary>
        /// Показывать кнопку действия
        /// </summary>
        public bool ShowActionButton { get; set; } = true;

        /// <summary>
        /// Показывать в системном трее
        /// </summary>
        public bool ShowInSystemTray { get; set; } = false;

        /// <summary>
        /// Звуковое уведомление
        /// </summary>
        public bool EnableSound { get; set; } = false;

        /// <summary>
        /// Вибрация (если поддерживается)
        /// </summary>
        public bool EnableVibration { get; set; } = false;
    }
}
