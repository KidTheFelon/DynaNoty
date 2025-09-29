namespace DynaNoty.Configuration
{
    /// <summary>
    /// Конфигурация производительности уведомлений
    /// </summary>
    public class NotificationPerformanceConfig
    {
        // === Производительность ===
        /// <summary>
        /// Максимальный размер пула уведомлений
        /// </summary>
        public int MaxPoolSize { get; set; } = 10;

        /// <summary>
        /// Количество предварительно созданных уведомлений
        /// </summary>
        public int PreWarmCount { get; set; } = 3;

        /// <summary>
        /// Включить кэширование для улучшения производительности
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Максимальный размер кэша
        /// </summary>
        public int MaxCacheSize { get; set; } = 100;

        // === Логирование ===
        /// <summary>
        /// Включить логирование
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Уровень логирования
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
