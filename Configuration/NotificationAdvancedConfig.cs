namespace DynaNoty.Configuration
{
    /// <summary>
    /// Продвинутые настройки уведомлений
    /// </summary>
    public class NotificationAdvancedConfig
    {
        // === Rate Limiting ===
        /// <summary>
        /// Максимальное количество уведомлений в минуту
        /// </summary>
        public int RateLimit { get; set; } = 20;

        /// <summary>
        /// Окно лимитирования в минутах
        /// </summary>
        public int RateLimitWindowMinutes { get; set; } = 1;

        // === Retry Logic ===
        /// <summary>
        /// Максимальное количество попыток повторной обработки
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Задержка между попытками в миллисекундах
        /// </summary>
        public int RetryDelayMs { get; set; } = 100;

        // === Text Validation ===
        /// <summary>
        /// Максимальная длина заголовка
        /// </summary>
        public int MaxTitleLength { get; set; } = 100;

        /// <summary>
        /// Максимальная длина подзаголовка
        /// </summary>
        public int MaxSubtitleLength { get; set; } = 200;

        /// <summary>
        /// Максимальная длина иконки
        /// </summary>
        public int MaxIconLength { get; set; } = 10;

        // === Fallback Values ===
        /// <summary>
        /// Иконка по умолчанию при пустом значении
        /// </summary>
        public string DefaultIcon { get; set; } = "🔔";

        /// <summary>
        /// Текст по умолчанию при пустом заголовке
        /// </summary>
        public string DefaultTitle { get; set; } = "Уведомление";

        /// <summary>
        /// Иконка для звонков
        /// </summary>
        public string CallIcon { get; set; } = "📞";

        /// <summary>
        /// Иконка для музыки
        /// </summary>
        public string MusicIcon { get; set; } = "🎵";

        // === Monitoring ===
        /// <summary>
        /// Интервал отчетов производительности в секундах
        /// </summary>
        public int PerformanceReportIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Включить детальную статистику
        /// </summary>
        public bool EnableDetailedStatistics { get; set; } = false;

        // === Actions ===
        /// <summary>
        /// Максимальное количество кнопок действий
        /// </summary>
        public int MaxActionButtons { get; set; } = 2;

        /// <summary>
        /// Показывать дополнительные действия в выпадающем меню
        /// </summary>
        public bool ShowAdditionalActionsInDropdown { get; set; } = false;
    }
}

