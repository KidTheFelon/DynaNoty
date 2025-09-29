using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;

namespace DynaNoty.Services
{
    /// <summary>
    /// Кэш для анимаций для улучшения производительности
    /// </summary>
    public class AnimationCache : IAnimationCache
    {
        private readonly ConcurrentDictionary<string, Storyboard> _storyboardCache = new();
        private readonly ConcurrentDictionary<string, DoubleAnimation> _doubleAnimationCache = new();
        private readonly ILogger<AnimationCache> _logger;
        private bool _disposed = false;

        public AnimationCache(ILogger<AnimationCache> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Получает или создает Storyboard для анимации появления
        /// </summary>
        public Storyboard GetAppearStoryboard(DependencyObject target, TimeSpan duration)
        {
            var key = $"appear_{duration.TotalMilliseconds}";
            
            return _storyboardCache.GetOrAdd(key, _ =>
            {
                var storyboard = new Storyboard();
                
                // Анимация масштаба
                var scaleXAnimation = new DoubleAnimation(0.8, 1.0, duration)
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut },
                    FillBehavior = FillBehavior.Stop
                };
                
                var scaleYAnimation = new DoubleAnimation(0.8, 1.0, duration)
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut },
                    FillBehavior = FillBehavior.Stop
                };
                
                // Анимация прозрачности
                var opacityAnimation = new DoubleAnimation(0, 1, duration)
                {
                    FillBehavior = FillBehavior.Stop
                };
                
                Storyboard.SetTarget(scaleXAnimation, target);
                Storyboard.SetTarget(scaleYAnimation, target);
                Storyboard.SetTarget(opacityAnimation, target);
                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
                
                storyboard.Children.Add(scaleXAnimation);
                storyboard.Children.Add(scaleYAnimation);
                storyboard.Children.Add(opacityAnimation);
                
                _logger?.LogDebug("Создан кэшированный Storyboard для анимации появления");
                return storyboard;
            });
        }

        /// <summary>
        /// Получает или создает Storyboard для анимации исчезновения
        /// </summary>
        public Storyboard GetDismissStoryboard(DependencyObject target, TimeSpan duration)
        {
            var key = $"dismiss_{duration.TotalMilliseconds}";
            
            return _storyboardCache.GetOrAdd(key, _ =>
            {
                var storyboard = new Storyboard();
                
                // Анимация масштаба (более плавная)
                var scaleXAnimation = new DoubleAnimation(1.0, 0.7, duration)
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseInOut },
                    FillBehavior = FillBehavior.Stop
                };
                
                var scaleYAnimation = new DoubleAnimation(1.0, 0.7, duration)
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseInOut },
                    FillBehavior = FillBehavior.Stop
                };
                
                // Анимация прозрачности (более плавная)
                var opacityAnimation = new DoubleAnimation(1, 0, duration)
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseInOut },
                    FillBehavior = FillBehavior.Stop
                };
                
                Storyboard.SetTarget(scaleXAnimation, target);
                Storyboard.SetTarget(scaleYAnimation, target);
                Storyboard.SetTarget(opacityAnimation, target);
                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
                
                storyboard.Children.Add(scaleXAnimation);
                storyboard.Children.Add(scaleYAnimation);
                storyboard.Children.Add(opacityAnimation);
                
                _logger?.LogDebug("Создан кэшированный Storyboard для анимации исчезновения");
                return storyboard;
            });
        }

        /// <summary>
        /// Получает или создает анимацию расширения ширины
        /// </summary>
        public DoubleAnimation GetWidthExpandAnimation(double fromWidth, double toWidth, TimeSpan duration)
        {
            var key = $"width_expand_{fromWidth}_{toWidth}_{duration.TotalMilliseconds}";
            
            return _doubleAnimationCache.GetOrAdd(key, _ =>
            {
                var animation = new DoubleAnimation(fromWidth, toWidth, duration)
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut },
                    FillBehavior = FillBehavior.Stop
                };
                
                _logger?.LogDebug("Создана кэшированная анимация расширения ширины");
                return animation;
            });
        }

        /// <summary>
        /// Получает или создает анимацию сжатия ширины
        /// </summary>
        public DoubleAnimation GetWidthCompactAnimation(double fromWidth, double toWidth, TimeSpan duration)
        {
            var key = $"width_compact_{fromWidth}_{toWidth}_{duration.TotalMilliseconds}";
            
            return _doubleAnimationCache.GetOrAdd(key, _ =>
            {
                var animation = new DoubleAnimation(fromWidth, toWidth, duration)
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseIn },
                    FillBehavior = FillBehavior.Stop
                };
                
                _logger?.LogDebug("Создана кэшированная анимация сжатия ширины");
                return animation;
            });
        }

        /// <summary>
        /// Очищает кэш
        /// </summary>
        public void ClearCache()
        {
            foreach (var storyboard in _storyboardCache.Values)
            {
                storyboard?.Stop();
            }
            
            _storyboardCache.Clear();
            _doubleAnimationCache.Clear();
            
            _logger?.LogInformation("Кэш анимаций очищен");
        }

        /// <summary>
        /// Получает статистику кэша
        /// </summary>
        public (int StoryboardCount, int DoubleAnimationCount) GetCacheStats()
        {
            return (_storyboardCache.Count, _doubleAnimationCache.Count);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                ClearCache();
                _disposed = true;
                _logger?.LogDebug("AnimationCache освобожден");
            }
        }
    }
}
