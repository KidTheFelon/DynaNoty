using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;

namespace DynaNoty.Services
{
    /// <summary>
    /// Оптимизированный сервис для анимаций с использованием CompositionTarget
    /// </summary>
    public class AnimationOptimizer : IAnimationOptimizer
    {
        private readonly ILogger<AnimationOptimizer> _logger;
        private readonly DispatcherTimer _frameTimer;
        private bool _isAnimating = false;
        private readonly object _lockObject = new object();

        public AnimationOptimizer(ILogger<AnimationOptimizer> logger = null)
        {
            _logger = logger;
            _frameTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _frameTimer.Tick += OnFrameTick;
        }

        /// <summary>
        /// Плавная анимация с использованием CompositionTarget для лучшей производительности
        /// </summary>
        public void AnimateProperty<T>(DependencyObject target, DependencyProperty property, 
            T fromValue, T toValue, TimeSpan duration, Action<T> onUpdate, Action onCompleted = null)
        {
            if (!_isAnimating)
            {
                StartAnimationLoop();
            }

            var animation = new SmoothAnimation<T>(target, property, fromValue, toValue, duration, onUpdate, onCompleted);
            animation.Start();
        }

        private void StartAnimationLoop()
        {
            lock (_lockObject)
            {
                if (!_isAnimating)
                {
                    _isAnimating = true;
                    _frameTimer.Start();
                    _logger?.LogDebug("Анимационный цикл запущен");
                }
            }
        }

        private void StopAnimationLoop()
        {
            lock (_lockObject)
            {
                if (_isAnimating)
                {
                    _isAnimating = false;
                    _frameTimer.Stop();
                    _logger?.LogDebug("Анимационный цикл остановлен");
                }
            }
        }

        private void OnFrameTick(object sender, EventArgs e)
        {
            // Обновление анимаций происходит через CompositionTarget
            CompositionTarget.Rendering += OnCompositionTargetRendering;
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= OnCompositionTargetRendering;
            // Здесь можно добавить логику для обновления анимаций
        }

        public void Dispose()
        {
            if (_frameTimer != null)
            {
                _frameTimer.Stop();
                _frameTimer.Tick -= OnFrameTick;
            }
        }
    }

    /// <summary>
    /// Плавная анимация с кастомной интерполяцией
    /// </summary>
    public class SmoothAnimation<T>
    {
        private readonly DependencyObject _target;
        private readonly DependencyProperty _property;
        private readonly T _fromValue;
        private readonly T _toValue;
        private readonly TimeSpan _duration;
        private readonly Action<T> _onUpdate;
        private readonly Action _onCompleted;
        private readonly DateTime _startTime;
        private bool _isRunning = false;

        public SmoothAnimation(DependencyObject target, DependencyProperty property, 
            T fromValue, T toValue, TimeSpan duration, Action<T> onUpdate, Action onCompleted = null)
        {
            _target = target;
            _property = property;
            _fromValue = fromValue;
            _toValue = toValue;
            _duration = duration;
            _onUpdate = onUpdate;
            _onCompleted = onCompleted;
            _startTime = DateTime.Now;
        }

        public void Start()
        {
            _isRunning = true;
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (!_isRunning) return;

            var elapsed = DateTime.Now - _startTime;
            var progress = Math.Min(elapsed.TotalMilliseconds / _duration.TotalMilliseconds, 1.0);

            // Применяем easing функцию (ease-out)
            var easedProgress = 1 - Math.Pow(1 - progress, 3);

            var currentValue = Interpolate(_fromValue, _toValue, easedProgress);
            _onUpdate?.Invoke(currentValue);

            if (progress >= 1.0)
            {
                _isRunning = false;
                CompositionTarget.Rendering -= OnRendering;
                _onCompleted?.Invoke();
            }
        }

        private T Interpolate(T from, T to, double progress)
        {
            if (typeof(T) == typeof(double))
            {
                var fromDouble = Convert.ToDouble(from);
                var toDouble = Convert.ToDouble(to);
                var result = fromDouble + (toDouble - fromDouble) * progress;
                return (T)Convert.ChangeType(result, typeof(T));
            }
            else if (typeof(T) == typeof(float))
            {
                var fromFloat = Convert.ToSingle(from);
                var toFloat = Convert.ToSingle(to);
                var result = fromFloat + (toFloat - fromFloat) * progress;
                return (T)Convert.ChangeType(result, typeof(T));
            }

            // Fallback для других типов
            return progress < 0.5 ? from : to;
        }
    }
}
