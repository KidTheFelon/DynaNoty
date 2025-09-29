using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using DynaNoty.Interfaces;
using DynaNoty.Configuration;

namespace DynaNoty.Services
{
    /// <summary>
    /// Базовый класс для уведомлений с общей функциональностью
    /// </summary>
    public abstract class BaseNotification : UserControl, IDisposable
    {
        protected readonly NotificationConfiguration _config;
        protected readonly ISystemThemeService _themeService;
        protected bool _disposed = false;

        public event EventHandler Dismissed;
        public event EventHandler ActionClicked;

        protected BaseNotification(NotificationConfiguration config, ISystemThemeService themeService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        }

        /// <summary>
        /// Применяет цвета с учетом системных настроек
        /// </summary>
        protected virtual void ApplyColors()
        {
            // Базовая реализация - переопределяется в наследниках
        }

        /// <summary>
        /// Анимация появления
        /// </summary>
        protected virtual void AnimateAppear()
        {
            if (!_config.EnableAnimations)
            {
                this.Opacity = 1.0;
                return;
            }

            // Останавливаем все активные анимации для предотвращения конфликтов
            this.BeginAnimation(OpacityProperty, null);
            if (this.RenderTransform is ScaleTransform existingTransform)
            {
                existingTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                existingTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            }

            // Оптимизированная анимация появления
            var scaleTransform = new ScaleTransform(0.8, 0.8);
            this.RenderTransform = scaleTransform;
            this.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleAnimation = new DoubleAnimation(0.8, 1.0, TimeSpan.FromMilliseconds(_config.AppearAnimationDuration))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };

            var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(_config.AppearAnimationDuration))
            {
                FillBehavior = FillBehavior.Stop
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            this.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        /// <summary>
        /// Анимация исчезновения
        /// </summary>
        protected virtual void AnimateDismiss()
        {
            System.Diagnostics.Debug.WriteLine("=== BaseNotification.AnimateDismiss НАЧАЛО ===");
            System.Diagnostics.Debug.WriteLine($"EnableAnimations: {_config.EnableAnimations}");

            if (!_config.EnableAnimations)
            {
                System.Diagnostics.Debug.WriteLine("Анимации отключены, мгновенное закрытие");
                this.Opacity = 0.0;
                OnDismissed();
                return;
            }

            // Сохраняем текущую позицию для восстановления после анимации
            var originalLeft = Canvas.GetLeft(this);
            var originalTop = Canvas.GetTop(this);
            System.Diagnostics.Debug.WriteLine($"Исходная позиция: Left={originalLeft}, Top={originalTop}");

            // Используем настройку длительности анимации закрытия
            var dismissDuration = _config.DismissAnimationDuration;
            System.Diagnostics.Debug.WriteLine($"Параметры: Duration={dismissDuration}ms, SlideDistance={_config.DismissSlideDistance}px");

            // Останавливаем анимации позиционирования и фиксируем горизонтальную позицию
            this.BeginAnimation(Canvas.LeftProperty, null);
            this.BeginAnimation(Canvas.TopProperty, null);
            System.Diagnostics.Debug.WriteLine("BaseNotification.AnimateDismiss: Остановлены анимации позиционирования");
            Canvas.SetLeft(this, originalLeft);
            System.Diagnostics.Debug.WriteLine($"BaseNotification.AnimateDismiss: Зафиксирована позиция Left={originalLeft}");

            // Создаем анимацию сдвига вверх с прозрачностью
            var opacityAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(dismissDuration))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.Stop
            };
            System.Diagnostics.Debug.WriteLine("Создана анимация прозрачности: от 1.0 до 0.0");

            // Анимация сдвига вверх
            var currentTop = Canvas.GetTop(this);
            var targetTop = currentTop - _config.DismissSlideDistance;
            var topAnimation = new DoubleAnimation(currentTop, targetTop, TimeSpan.FromMilliseconds(dismissDuration))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.Stop
            };
            System.Diagnostics.Debug.WriteLine($"Создана анимация сдвига: от {currentTop} до {targetTop}");

            try
            {
                System.Diagnostics.Debug.WriteLine("Запускаем анимации...");
                this.BeginAnimation(OpacityProperty, opacityAnimation);
                System.Diagnostics.Debug.WriteLine("Запущена анимация прозрачности");

                this.BeginAnimation(Canvas.TopProperty, topAnimation);
                System.Diagnostics.Debug.WriteLine("Запущена анимация сдвига");

                System.Diagnostics.Debug.WriteLine("=== АНИМАЦИИ ЗАПУЩЕНЫ УСПЕШНО ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== ОШИБКА ЗАПУСКА АНИМАЦИИ: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine("Используем graceful degradation");
                // Graceful degradation - мгновенное закрытие
                this.Opacity = 0.0;
                Canvas.SetLeft(this, originalLeft);
                Canvas.SetTop(this, originalTop);
                System.Diagnostics.Debug.WriteLine("Graceful degradation выполнен");
                OnDismissed();
                return;
            }

            // Задержка перед вызовом события
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(dismissDuration)
            };
            timer.Tick += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("=== ТАЙМЕР АНИМАЦИИ СРАБОТАЛ ===");
                timer.Stop();
                // Восстанавливаем исходную позицию
                System.Diagnostics.Debug.WriteLine($"Восстанавливаем позицию: Left={originalLeft}, Top={originalTop}");
                Canvas.SetLeft(this, originalLeft);
                Canvas.SetTop(this, originalTop);
                System.Diagnostics.Debug.WriteLine("Позиция восстановлена, вызываем OnDismissed");
                OnDismissed();
                System.Diagnostics.Debug.WriteLine("=== BaseNotification.AnimateDismiss ЗАВЕРШЕНО ===");
            };
            timer.Start();
        }

        /// <summary>
        /// Вызывает событие закрытия
        /// </summary>
        protected virtual void OnDismissed()
        {
            Dismissed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Вызывает событие нажатия действия
        /// </summary>
        protected virtual void OnActionClicked()
        {
            ActionClicked?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Закрывает уведомление
        /// </summary>
        public virtual void Dismiss()
        {
            if (_disposed) return;
            AnimateDismiss();
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Dismissed = null;
                ActionClicked = null;
            }
        }
    }
}
