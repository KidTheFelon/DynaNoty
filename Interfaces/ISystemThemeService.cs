using System.Windows.Media;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с системной темой и акцентными цветами
    /// </summary>
    public interface ISystemThemeService
    {
        /// <summary>
        /// Определяет, включена ли темная тема в системе
        /// </summary>
        bool IsDarkTheme { get; }

        /// <summary>
        /// Получает акцентный цвет системы
        /// </summary>
        Color AccentColor { get; }

        /// <summary>
        /// Получает рекомендуемый цвет фона для уведомлений
        /// </summary>
        Color GetRecommendedBackgroundColor();

        /// <summary>
        /// Получает рекомендуемый цвет текста для уведомлений
        /// </summary>
        Color GetRecommendedTextColor();

        /// <summary>
        /// Получает рекомендуемый цвет иконки для уведомлений
        /// </summary>
        Color GetRecommendedIconColor();

        /// <summary>
        /// Обновляет определение темы и акцентного цвета
        /// </summary>
        void Refresh();

        /// <summary>
        /// Событие изменения системной темы
        /// </summary>
        event System.EventHandler SystemThemeChanged;
    }
}
