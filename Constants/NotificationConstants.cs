namespace DynaNoty.Constants
{
    /// <summary>
    /// Константы для системы уведомлений
    /// </summary>
    public static class NotificationConstants
    {
        // === Размеры ===
        public const double DEFAULT_MAX_WIDTH = 300.0;
        public const double DEFAULT_MIN_WIDTH = 60.0;
        public const double DEFAULT_HEIGHT = 60.0;
        public const double DEFAULT_TOP_MARGIN = 25.0;
        public const double DEFAULT_VERTICAL_SPACING = 70.0;
        public const double DEFAULT_CORNER_RADIUS = 20.0;

        // === Анимации ===
        public const int DEFAULT_APPEAR_DURATION = 300;
        public const int DEFAULT_EXPAND_DURATION = 400;
        public const int DEFAULT_REPOSITION_DURATION = 300;
        public const int DEFAULT_EXPAND_DELAY = 500;
        public const int DEFAULT_DISMISS_DURATION = 200;

        // === Поведение ===
        public const int DEFAULT_AUTO_HIDE_TIMEOUT = 4;
        public const int DEFAULT_CLEANUP_INTERVAL = 1;
        public const int DEFAULT_MAX_NOTIFICATIONS = 3;

        // === Производительность ===
        public const int DEFAULT_MAX_POOL_SIZE = 10;
        public const int DEFAULT_PRE_WARM_COUNT = 3;
        public const int DEFAULT_MAX_CACHE_SIZE = 100;
        public const int DEFAULT_MAX_RETRIES = 3;
        public const int DEFAULT_RATE_LIMIT = 20;
        public const int DEFAULT_RATE_LIMIT_WINDOW_MINUTES = 1;

        // === Валидация (теперь настраиваемые) ===
        public const int DEFAULT_MAX_TITLE_LENGTH = 100;
        public const int DEFAULT_MAX_SUBTITLE_LENGTH = 200;
        public const int DEFAULT_MAX_ICON_LENGTH = 10;

        // === Обратная совместимость ===
        public const int MAX_STRING_LENGTH = DEFAULT_MAX_TITLE_LENGTH;
        public const int MAX_ICON_LENGTH = DEFAULT_MAX_ICON_LENGTH;

        // === Размеры элементов ===
        public const double DEFAULT_ICON_SIZE = 24.0;
        public const double DEFAULT_BUTTON_SIZE = 24.0;

        // === Иконки ===
        public const string DEFAULT_ICON = "🔔";
        public const string CALL_ICON = "📞";
        public const string MUSIC_ICON = "🎵";

        // === Таймауты ===
        public const int RETRY_DELAY_MS = 100;
        public const int PERFORMANCE_REPORT_INTERVAL_SECONDS = 30;
    }
}
