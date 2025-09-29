using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для кэширования анимаций
    /// </summary>
    public interface IAnimationCache : IDisposable
    {
        /// <summary>
        /// Получает или создает Storyboard для анимации появления
        /// </summary>
        Storyboard GetAppearStoryboard(DependencyObject target, TimeSpan duration);

        /// <summary>
        /// Получает или создает Storyboard для анимации исчезновения
        /// </summary>
        Storyboard GetDismissStoryboard(DependencyObject target, TimeSpan duration);

        /// <summary>
        /// Получает или создает анимацию расширения ширины
        /// </summary>
        DoubleAnimation GetWidthExpandAnimation(double fromWidth, double toWidth, TimeSpan duration);

        /// <summary>
        /// Получает или создает анимацию сжатия ширины
        /// </summary>
        DoubleAnimation GetWidthCompactAnimation(double fromWidth, double toWidth, TimeSpan duration);

        /// <summary>
        /// Очищает кэш
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Получает статистику кэша
        /// </summary>
        (int StoryboardCount, int DoubleAnimationCount) GetCacheStats();
    }
}
