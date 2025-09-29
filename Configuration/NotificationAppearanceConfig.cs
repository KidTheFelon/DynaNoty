using System.Windows.Media;

namespace DynaNoty.Configuration
{
    /// <summary>
    /// Конфигурация внешнего вида уведомлений
    /// </summary>
    public class NotificationAppearanceConfig
    {
        // === Размеры ===
        /// <summary>
        /// Максимальная ширина уведомления (пиксели)
        /// </summary>
        public double MaxNotificationWidth { get; set; } = 400;

        /// <summary>
        /// Минимальная ширина уведомления (пиксели)
        /// </summary>
        public double MinNotificationWidth { get; set; } = 60;

        /// <summary>
        /// Высота уведомления (пиксели)
        /// </summary>
        public double NotificationHeight { get; set; } = 60;

        /// <summary>
        /// Высота уведомления при первом раскрытии (пиксели)
        /// </summary>
        public double ExpandedNotificationHeight { get; set; } = 80;

        /// <summary>
        /// Минимальная высота уведомления при полном раскрытии (пиксели)
        /// </summary>
        public double FullyExpandedMinHeight { get; set; } = 135;

        /// <summary>
        /// Максимальная высота уведомления (пиксели)
        /// </summary>
        public double MaxNotificationHeight { get; set; } = 400;

        /// <summary>
        /// Базовая высота для полностью раскрытого уведомления (пиксели)
        /// </summary>
        public double FullyExpandedBaseHeight { get; set; } = 160;

        /// <summary>
        /// Высота панели действий (пиксели)
        /// </summary>
        public double ActionsPanelHeight { get; set; } = 50;

        /// <summary>
        /// Размер иконки
        /// </summary>
        public double IconSize { get; set; } = 24;

        /// <summary>
        /// Размер кнопки действия (пиксели)
        /// </summary>
        public double ActionButtonSize { get; set; } = 24;

        // === Позиционирование ===
        /// <summary>
        /// Отступ от верха экрана (пиксели)
        /// </summary>
        public double TopMargin { get; set; } = 20;

        /// <summary>
        /// Расстояние между уведомлениями (пиксели)
        /// </summary>
        public double VerticalSpacing { get; set; } = 10;

        /// <summary>
        /// Высота области уведомлений (пиксели)
        /// </summary>
        public double NotificationAreaHeight { get; set; } = 250;

        // === Цвета ===
        /// <summary>
        /// Цвет фона уведомления
        /// </summary>
        public Color BackgroundColor { get; set; } = Colors.Black;

        /// <summary>
        /// Цвет текста
        /// </summary>
        public Color TextColor { get; set; } = Colors.White;

        /// <summary>
        /// Цвет иконки
        /// </summary>
        public Color IconColor { get; set; } = Colors.White;

        /// <summary>
        /// Радиус скругления углов (пиксели)
        /// </summary>
        public double CornerRadius { get; set; } = 20;

        // === Шрифты ===
        /// <summary>
        /// Размер шрифта заголовка
        /// </summary>
        public double TitleFontSize { get; set; } = 16;

        /// <summary>
        /// Размер шрифта подзаголовка
        /// </summary>
        public double SubtitleFontSize { get; set; } = 14;

        /// <summary>
        /// Размер иконки
        /// </summary>
        public double IconFontSize { get; set; } = 20;

        // === Тема ===
        /// <summary>
        /// Тема уведомлений
        /// </summary>
        public NotificationTheme Theme { get; set; } = NotificationTheme.System;

        /// <summary>
        /// Автоматически подстраиваться под системную тему
        /// </summary>
        public bool AutoAdaptToSystemTheme { get; set; } = true;

        /// <summary>
        /// Использовать акцентный цвет системы для иконок
        /// </summary>
        public bool UseSystemAccentColor { get; set; } = true;

        /// <summary>
        /// Приоритет системных настроек над пользовательскими
        /// </summary>
        public bool SystemSettingsOverride { get; set; } = false;
    }
}
