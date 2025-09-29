using System;
using DynaNoty.Constants;

namespace DynaNoty.Configuration
{
    /// <summary>
    /// Состояние уведомления
    /// </summary>
    public enum NotificationState
    {
        /// <summary>
        /// Сжатое состояние (только иконка)
        /// </summary>
        Compact,
        
        /// <summary>
        /// Расширенное состояние (ширина + текст)
        /// </summary>
        Expanded,
        
        /// <summary>
        /// Полностью раскрытое состояние (ширина + высота + полный текст + действия)
        /// </summary>
        FullyExpanded
    }

    /// <summary>
    /// Конфигурация для системы уведомлений
    /// </summary>
    public class NotificationConfiguration
    {
        /// <summary>
        /// Конфигурация внешнего вида
        /// </summary>
        public NotificationAppearanceConfig Appearance { get; set; } = new();

        /// <summary>
        /// Конфигурация поведения
        /// </summary>
        public NotificationBehaviorConfig Behavior { get; set; } = new();

        /// <summary>
        /// Конфигурация системных уведомлений
        /// </summary>
        public NotificationSystemConfig System { get; set; } = new();

        /// <summary>
        /// Конфигурация производительности
        /// </summary>
        public NotificationPerformanceConfig Performance { get; set; } = new();

        /// <summary>
        /// Продвинутые настройки
        /// </summary>
        public NotificationAdvancedConfig Advanced { get; set; } = new();

        // === Обратная совместимость ===
        /// <summary>
        /// Время автоматического скрытия уведомлений (секунды)
        /// </summary>
        public int AutoHideTimeoutSeconds 
        { 
            get => Behavior.AutoHideTimeoutSeconds; 
            set => Behavior.AutoHideTimeoutSeconds = value; 
        }

        /// <summary>
        /// Интервал очистки завершенных уведомлений (секунды)
        /// </summary>
        public int CleanupIntervalSeconds 
        { 
            get => Behavior.CleanupIntervalSeconds; 
            set => Behavior.CleanupIntervalSeconds = value; 
        }

        /// <summary>
        /// Максимальное количество уведомлений одновременно
        /// </summary>
        public int MaxNotifications 
        { 
            get => Behavior.MaxNotifications; 
            set => Behavior.MaxNotifications = value; 
        }

        /// <summary>
        /// Максимальная ширина уведомления (пиксели)
        /// </summary>
        public double MaxNotificationWidth 
        { 
            get => Appearance.MaxNotificationWidth; 
            set => Appearance.MaxNotificationWidth = value; 
        }

        /// <summary>
        /// Минимальная ширина уведомления (пиксели)
        /// </summary>
        public double MinNotificationWidth 
        { 
            get => Appearance.MinNotificationWidth; 
            set => Appearance.MinNotificationWidth = value; 
        }

        /// <summary>
        /// Высота уведомления (пиксели)
        /// </summary>
        public double NotificationHeight 
        { 
            get => Appearance.NotificationHeight; 
            set => Appearance.NotificationHeight = value; 
        }

        /// <summary>
        /// Минимальная высота уведомления (пиксели)
        /// </summary>
        public double MinNotificationHeight 
        { 
            get => Appearance.NotificationHeight; 
            set => Appearance.NotificationHeight = value; 
        }

        /// <summary>
        /// Высота уведомления при первом раскрытии (пиксели)
        /// </summary>
        public double ExpandedNotificationHeight 
        { 
            get => Appearance.ExpandedNotificationHeight; 
            set => Appearance.ExpandedNotificationHeight = value; 
        }

        /// <summary>
        /// Минимальная высота уведомления при полном раскрытии (пиксели)
        /// </summary>
        public double FullyExpandedMinHeight 
        { 
            get => Appearance.FullyExpandedMinHeight; 
            set => Appearance.FullyExpandedMinHeight = value; 
        }

        /// <summary>
        /// Максимальная высота уведомления (пиксели)
        /// </summary>
        public double MaxNotificationHeight 
        { 
            get => Appearance.MaxNotificationHeight; 
            set => Appearance.MaxNotificationHeight = value; 
        }

        /// <summary>
        /// Базовая высота для полностью раскрытого уведомления (пиксели)
        /// </summary>
        public double FullyExpandedBaseHeight 
        { 
            get => Appearance.FullyExpandedBaseHeight; 
            set => Appearance.FullyExpandedBaseHeight = value; 
        }

        /// <summary>
        /// Высота панели действий (пиксели)
        /// </summary>
        public double ActionsPanelHeight 
        { 
            get => Appearance.ActionsPanelHeight; 
            set => Appearance.ActionsPanelHeight = value; 
        }

        /// <summary>
        /// Отступ от верха экрана (пиксели)
        /// </summary>
        public double TopMargin 
        { 
            get => Appearance.TopMargin; 
            set => Appearance.TopMargin = value; 
        }

        /// <summary>
        /// Расстояние между уведомлениями (пиксели)
        /// </summary>
        public double VerticalSpacing 
        { 
            get => Appearance.VerticalSpacing; 
            set => Appearance.VerticalSpacing = value; 
        }

        /// <summary>
        /// Высота области уведомлений (пиксели)
        /// </summary>
        public double NotificationAreaHeight 
        { 
            get => Appearance.NotificationAreaHeight; 
            set => Appearance.NotificationAreaHeight = value; 
        }

        /// <summary>
        /// Включить анимации
        /// </summary>
        public bool EnableAnimations 
        { 
            get => Behavior.EnableAnimations; 
            set => Behavior.EnableAnimations = value; 
        }

        /// <summary>
        /// Длительность анимации появления (миллисекунды)
        /// </summary>
        public int AppearAnimationDuration 
        { 
            get => Behavior.AppearAnimationDuration; 
            set => Behavior.AppearAnimationDuration = value; 
        }

        /// <summary>
        /// Длительность анимации расширения (миллисекунды)
        /// </summary>
        public int ExpandAnimationDuration 
        { 
            get => Behavior.ExpandAnimationDuration; 
            set => Behavior.ExpandAnimationDuration = value; 
        }

        /// <summary>
        /// Длительность анимации сдвига (миллисекунды)
        /// </summary>
        public int RepositionAnimationDuration 
        { 
            get => Behavior.RepositionAnimationDuration; 
            set => Behavior.RepositionAnimationDuration = value; 
        }

        /// <summary>
        /// Задержка перед расширением (миллисекунды)
        /// </summary>
        public int ExpandDelay 
        { 
            get => Behavior.ExpandDelay; 
            set => Behavior.ExpandDelay = value; 
        }

        /// <summary>
        /// Длительность анимации закрытия (миллисекунды)
        /// </summary>
        public int DismissAnimationDuration 
        { 
            get => Behavior.DismissAnimationDuration; 
            set => Behavior.DismissAnimationDuration = value; 
        }

        /// <summary>
        /// Расстояние сдвига при закрытии (пиксели)
        /// </summary>
        public double DismissSlideDistance 
        { 
            get => Behavior.DismissSlideDistance; 
            set => Behavior.DismissSlideDistance = value; 
        }

        /// <summary>
        /// Время отображения свернутого уведомления (миллисекунды)
        /// </summary>
        public int CompactDisplayDuration 
        { 
            get => Behavior.CompactDisplayDuration; 
            set => Behavior.CompactDisplayDuration = value; 
        }

        /// <summary>
        /// Время отображения расширенного уведомления (миллисекунды)
        /// </summary>
        public int ExpandedDisplayDuration 
        { 
            get => Behavior.ExpandedDisplayDuration; 
            set => Behavior.ExpandedDisplayDuration = value; 
        }

        /// <summary>
        /// Время отображения полностью раскрытого уведомления (миллисекунды)
        /// </summary>
        public int FullyExpandedDisplayDuration 
        { 
            get => Behavior.FullyExpandedDisplayDuration; 
            set => Behavior.FullyExpandedDisplayDuration = value; 
        }

        /// <summary>
        /// Включить автораскрытие уведомлений
        /// </summary>
        public bool EnableAutoExpand 
        { 
            get => Behavior.EnableAutoExpand; 
            set => Behavior.EnableAutoExpand = value; 
        }

        /// <summary>
        /// Цвет фона уведомления
        /// </summary>
        public System.Windows.Media.Color BackgroundColor 
        { 
            get => Appearance.BackgroundColor; 
            set => Appearance.BackgroundColor = value; 
        }

        /// <summary>
        /// Цвет текста
        /// </summary>
        public System.Windows.Media.Color TextColor 
        { 
            get => Appearance.TextColor; 
            set => Appearance.TextColor = value; 
        }

        /// <summary>
        /// Цвет иконки
        /// </summary>
        public System.Windows.Media.Color IconColor 
        { 
            get => Appearance.IconColor; 
            set => Appearance.IconColor = value; 
        }

        /// <summary>
        /// Радиус скругления углов (пиксели)
        /// </summary>
        public double CornerRadius 
        { 
            get => Appearance.CornerRadius; 
            set => Appearance.CornerRadius = value; 
        }

        /// <summary>
        /// Размер шрифта заголовка
        /// </summary>
        public double TitleFontSize 
        { 
            get => Appearance.TitleFontSize; 
            set => Appearance.TitleFontSize = value; 
        }

        /// <summary>
        /// Размер шрифта подзаголовка
        /// </summary>
        public double SubtitleFontSize 
        { 
            get => Appearance.SubtitleFontSize; 
            set => Appearance.SubtitleFontSize = value; 
        }

        /// <summary>
        /// Размер иконки
        /// </summary>
        public double IconFontSize 
        { 
            get => Appearance.IconFontSize; 
            set => Appearance.IconFontSize = value; 
        }

        /// <summary>
        /// Размер контейнера иконки (пиксели)
        /// </summary>
        public double IconSize 
        { 
            get => Appearance.IconSize; 
            set => Appearance.IconSize = value; 
        }

        /// <summary>
        /// Размер кнопки действия (пиксели)
        /// </summary>
        public double ActionButtonSize 
        { 
            get => Appearance.ActionButtonSize; 
            set => Appearance.ActionButtonSize = value; 
        }

        /// <summary>
        /// Показывать кнопку действия
        /// </summary>
        public bool ShowActionButton 
        { 
            get => Behavior.ShowActionButton; 
            set => Behavior.ShowActionButton = value; 
        }

        /// <summary>
        /// Показывать в системном трее
        /// </summary>
        public bool ShowInSystemTray 
        { 
            get => Behavior.ShowInSystemTray; 
            set => Behavior.ShowInSystemTray = value; 
        }

        /// <summary>
        /// Звуковое уведомление
        /// </summary>
        public bool EnableSound 
        { 
            get => Behavior.EnableSound; 
            set => Behavior.EnableSound = value; 
        }

        /// <summary>
        /// Вибрация (если поддерживается)
        /// </summary>
        public bool EnableVibration 
        { 
            get => Behavior.EnableVibration; 
            set => Behavior.EnableVibration = value; 
        }

        /// <summary>
        /// Автоматически подстраиваться под системную тему
        /// </summary>
        public bool AutoAdaptToSystemTheme 
        { 
            get => Appearance.AutoAdaptToSystemTheme; 
            set => Appearance.AutoAdaptToSystemTheme = value; 
        }

        /// <summary>
        /// Использовать акцентный цвет системы для иконок
        /// </summary>
        public bool UseSystemAccentColor 
        { 
            get => Appearance.UseSystemAccentColor; 
            set => Appearance.UseSystemAccentColor = value; 
        }

        /// <summary>
        /// Приоритет системных настроек над пользовательскими
        /// </summary>
        public bool SystemSettingsOverride 
        { 
            get => Appearance.SystemSettingsOverride; 
            set => Appearance.SystemSettingsOverride = value; 
        }

        /// <summary>
        /// Максимальный размер пула уведомлений
        /// </summary>
        public int MaxPoolSize 
        { 
            get => Performance.MaxPoolSize; 
            set => Performance.MaxPoolSize = value; 
        }

        /// <summary>
        /// Количество предварительно созданных уведомлений
        /// </summary>
        public int PreWarmCount 
        { 
            get => Performance.PreWarmCount; 
            set => Performance.PreWarmCount = value; 
        }

        /// <summary>
        /// Включить кэширование для улучшения производительности
        /// </summary>
        public bool EnableCaching 
        { 
            get => Performance.EnableCaching; 
            set => Performance.EnableCaching = value; 
        }

        /// <summary>
        /// Максимальный размер кэша
        /// </summary>
        public int MaxCacheSize 
        { 
            get => Performance.MaxCacheSize; 
            set => Performance.MaxCacheSize = value; 
        }

        /// <summary>
        /// Включить логирование
        /// </summary>
        public bool EnableLogging 
        { 
            get => Performance.EnableLogging; 
            set => Performance.EnableLogging = value; 
        }

        /// <summary>
        /// Уровень логирования
        /// </summary>
        public LogLevel LogLevel 
        { 
            get => Performance.LogLevel; 
            set => Performance.LogLevel = value; 
        }

        /// <summary>
        /// Включить системные уведомления Windows
        /// </summary>
        public bool EnableSystemNotifications 
        { 
            get => System.EnableSystemNotifications; 
            set => System.EnableSystemNotifications = value; 
        }

        /// <summary>
        /// Показывать системные уведомления только при свернутом приложении
        /// </summary>
        public bool SystemNotificationsOnlyWhenMinimized 
        { 
            get => System.SystemNotificationsOnlyWhenMinimized; 
            set => System.SystemNotificationsOnlyWhenMinimized = value; 
        }

        /// <summary>
        /// Показывать системные уведомления для всех типов уведомлений
        /// </summary>
        public bool SystemNotificationsForAllTypes 
        { 
            get => System.SystemNotificationsForAllTypes; 
            set => System.SystemNotificationsForAllTypes = value; 
        }

        /// <summary>
        /// Показывать системные уведомления только для важных уведомлений
        /// </summary>
        public bool SystemNotificationsForImportantOnly 
        { 
            get => System.SystemNotificationsForImportantOnly; 
            set => System.SystemNotificationsForImportantOnly = value; 
        }

        /// <summary>
        /// Время отображения системного уведомления (секунды)
        /// </summary>
        public int SystemNotificationDuration 
        { 
            get => System.SystemNotificationDuration; 
            set => System.SystemNotificationDuration = value; 
        }

        /// <summary>
        /// Включить звук для системных уведомлений
        /// </summary>
        public bool EnableSystemNotificationSound 
        { 
            get => System.EnableSystemNotificationSound; 
            set => System.EnableSystemNotificationSound = value; 
        }

        /// <summary>
        /// Включить действия в системных уведомлениях
        /// </summary>
        public bool EnableSystemNotificationActions 
        { 
            get => System.EnableSystemNotificationActions; 
            set => System.EnableSystemNotificationActions = value; 
        }

        /// <summary>
        /// Тема уведомлений
        /// </summary>
        public NotificationTheme Theme 
        { 
            get => Appearance.Theme; 
            set => Appearance.Theme = value; 
        }

        // === Продвинутые настройки (обратная совместимость) ===
        /// <summary>
        /// Максимальное количество уведомлений в минуту
        /// </summary>
        public int RateLimit 
        { 
            get => Advanced.RateLimit; 
            set => Advanced.RateLimit = value; 
        }

        /// <summary>
        /// Максимальное количество попыток повторной обработки
        /// </summary>
        public int MaxRetries 
        { 
            get => Advanced.MaxRetries; 
            set => Advanced.MaxRetries = value; 
        }

        /// <summary>
        /// Задержка между попытками в миллисекундах
        /// </summary>
        public int RetryDelayMs 
        { 
            get => Advanced.RetryDelayMs; 
            set => Advanced.RetryDelayMs = value; 
        }

        /// <summary>
        /// Максимальная длина заголовка
        /// </summary>
        public int MaxTitleLength 
        { 
            get => Advanced.MaxTitleLength; 
            set => Advanced.MaxTitleLength = value; 
        }

        /// <summary>
        /// Максимальная длина подзаголовка
        /// </summary>
        public int MaxSubtitleLength 
        { 
            get => Advanced.MaxSubtitleLength; 
            set => Advanced.MaxSubtitleLength = value; 
        }

        /// <summary>
        /// Иконка по умолчанию
        /// </summary>
        public string DefaultIcon 
        { 
            get => Advanced.DefaultIcon; 
            set => Advanced.DefaultIcon = value; 
        }

        /// <summary>
        /// Текст по умолчанию
        /// </summary>
        public string DefaultTitle 
        { 
            get => Advanced.DefaultTitle; 
            set => Advanced.DefaultTitle = value; 
        }
    }

    /// <summary>
    /// Уровни логирования
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Information,
        Warning,
        Error
    }

    /// <summary>
    /// Готовые темы для уведомлений
    /// </summary>
    public enum NotificationTheme
    {
        /// <summary>
        /// Системная тема (автоматическая)
        /// </summary>
        System,
        
        /// <summary>
        /// Темная тема
        /// </summary>
        Dark,
        
        /// <summary>
        /// Светлая тема
        /// </summary>
        Light,
        
        /// <summary>
        /// Синяя тема
        /// </summary>
        Blue,
        
        /// <summary>
        /// Зеленая тема
        /// </summary>
        Green,
        
        /// <summary>
        /// Фиолетовая тема
        /// </summary>
        Purple,
        
        /// <summary>
        /// Оранжевая тема
        /// </summary>
        Orange,
        
        /// <summary>
        /// Розовая тема
        /// </summary>
        Pink,
        
        /// <summary>
        /// Пользовательская тема (настраиваемая)
        /// </summary>
        Custom
    }
}
