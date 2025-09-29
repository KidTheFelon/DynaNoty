using System;
using System.Windows;
using System.Windows.Media.Animation;
using DynaNoty.Configuration;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Фабрика для создания анимаций
    /// </summary>
    public class AnimationFactory
    {
        private readonly NotificationConfiguration _config;
        private readonly ILogger _logger;

        public AnimationFactory(NotificationConfiguration config, ILogger logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
        }

        /// <summary>
        /// Создает анимацию масштабирования
        /// </summary>
        public DoubleAnimation CreateScaleAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };
        }

        /// <summary>
        /// Создает анимацию прозрачности
        /// </summary>
        public DoubleAnimation CreateOpacityAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                FillBehavior = FillBehavior.Stop
            };
        }

        /// <summary>
        /// Создает анимацию ширины
        /// </summary>
        public DoubleAnimation CreateWidthAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        /// <summary>
        /// Создает анимацию высоты
        /// </summary>
        public DoubleAnimation CreateHeightAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        /// <summary>
        /// Создает анимацию появления
        /// </summary>
        public (DoubleAnimation ScaleX, DoubleAnimation ScaleY, DoubleAnimation Opacity) CreateAppearAnimations()
        {
            var scaleAnimation = CreateScaleAnimation(0.8, 1.0, _config.AppearAnimationDuration);
            var opacityAnimation = CreateOpacityAnimation(0, 1, _config.AppearAnimationDuration);

            return (scaleAnimation, scaleAnimation, opacityAnimation);
        }

        /// <summary>
        /// Создает анимацию расширения
        /// </summary>
        public (DoubleAnimation Width, DoubleAnimation Height, DoubleAnimation ContentOpacity, DoubleAnimation ActionOpacity) CreateExpandAnimations(
            double currentHeight, double targetHeight)
        {
            var widthAnimation = CreateWidthAnimation(
                _config.MinNotificationWidth, 
                _config.MaxNotificationWidth, 
                _config.ExpandAnimationDuration);

            var heightAnimation = CreateHeightAnimation(currentHeight, targetHeight, _config.ExpandAnimationDuration);
            
            var contentOpacityAnimation = CreateOpacityAnimation(0, 1, _config.ExpandAnimationDuration);
            contentOpacityAnimation.BeginTime = TimeSpan.FromMilliseconds(200);
            
            var actionOpacityAnimation = CreateOpacityAnimation(0, 1, _config.ExpandAnimationDuration);
            actionOpacityAnimation.BeginTime = TimeSpan.FromMilliseconds(300);

            return (widthAnimation, heightAnimation, contentOpacityAnimation, actionOpacityAnimation);
        }

        /// <summary>
        /// Создает анимацию сжатия
        /// </summary>
        public (DoubleAnimation Width, DoubleAnimation ContentOpacity) CreateCompactAnimations()
        {
            var widthAnimation = CreateWidthAnimation(
                _config.MaxNotificationWidth, 
                _config.MinNotificationWidth, 
                300);

            var contentOpacityAnimation = CreateOpacityAnimation(1, 0, 200);

            return (widthAnimation, contentOpacityAnimation);
        }

        /// <summary>
        /// Создает анимацию исчезновения
        /// </summary>
        public (DoubleAnimation ScaleX, DoubleAnimation ScaleY, DoubleAnimation Opacity) CreateDismissAnimations()
        {
            var scaleAnimation = CreateScaleAnimation(1.0, 0.8, 200);
            var opacityAnimation = CreateOpacityAnimation(1, 0, 200);

            return (scaleAnimation, scaleAnimation, opacityAnimation);
        }

        /// <summary>
        /// Создает анимацию полного раскрытия
        /// </summary>
        public DoubleAnimation CreateFullyExpandAnimation(double currentHeight, double targetHeight)
        {
            return CreateHeightAnimation(currentHeight, targetHeight, _config.ExpandAnimationDuration);
        }

        /// <summary>
        /// Создает анимацию перепозиционирования
        /// </summary>
        public DoubleAnimation CreateRepositionAnimation(double from, double to)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(_config.RepositionAnimationDuration))
            {
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }
    }
}