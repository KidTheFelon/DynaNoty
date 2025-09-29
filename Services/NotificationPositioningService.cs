using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DynaNoty.Configuration;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для позиционирования уведомлений
    /// </summary>
    public class NotificationPositioningService : INotificationPositioningService
    {
        private readonly NotificationConfiguration _config;
        private readonly ILogger<NotificationPositioningService> _logger;

        public NotificationPositioningService(NotificationConfiguration config, ILogger<NotificationPositioningService> logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
        }

        /// <summary>
        /// Позиционирует уведомление на экране
        /// </summary>
        public void PositionNotification(UIElement notification)
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                // Оптимизированное обновление layout
                var frameworkElement = notification as FrameworkElement;
                if (frameworkElement != null)
                {
                    OptimizedLayoutUpdate(frameworkElement);
                }

                // Используем реальную ширину уведомления для правильного центрирования
                var notificationWidth = GetNotificationWidth(notification);
                var notificationHeight = GetNotificationHeight(notification);

                // Рассчитываем позицию с проверкой границ экрана
                var left = CalculateHorizontalPosition(screenWidth, notificationWidth);
                var top = Math.Max(0, _config.TopMargin); // Не выходим за верхнюю границу

                // Проверяем, не выходит ли уведомление за нижнюю границу
                if (top + notificationHeight > screenHeight - 50) // 50px отступ снизу
                {
                    top = Math.Max(0, screenHeight - notificationHeight - 50);
                    _logger?.LogWarning("Уведомление размещено в нижней части экрана из-за нехватки места");
                }

                Canvas.SetLeft(notification, left);
                Canvas.SetTop(notification, top);

                _logger?.LogDebug("Уведомление позиционировано: Left={Left}, Top={Top}, Width={Width}, Height={Height}",
                    left, top, notificationWidth, notificationHeight);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка позиционирования уведомления");
                throw;
            }
        }

        /// <summary>
        /// Вычисляет позицию для множественных уведомлений
        /// </summary>
        public void PositionNotification(UIElement notification, int index)
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                // Оптимизированное обновление layout
                if (notification is FrameworkElement frameworkElement)
                {
                    OptimizedLayoutUpdate(frameworkElement);
                }

                // Используем реальную ширину уведомления для правильного центрирования
                var notificationWidth = GetNotificationWidth(notification);
                var notificationHeight = GetNotificationHeight(notification);
                var left = CalculateHorizontalPosition(screenWidth, notificationWidth);

                // Рассчитываем позицию с учетом реальной высоты уведомлений
                var top = CalculateTopPosition(notification, index);

                // Проверяем границы экрана
                if (top + notificationHeight > screenHeight - 50) // 50px отступ снизу
                {
                    top = Math.Max(0, screenHeight - notificationHeight - 50);
                    _logger?.LogWarning("Уведомление {Index} размещено в нижней части экрана из-за нехватки места", index);
                }

                Canvas.SetLeft(notification, left);
                Canvas.SetTop(notification, top);

                // Убеждаемся, что уведомление видимо
                notification.Visibility = Visibility.Visible;

                _logger?.LogDebug("Уведомление {Index} позиционировано: Left={Left}, Top={Top}, Width={Width}, Height={Height}, ScreenWidth={ScreenWidth}",
                    index, left, top, notificationWidth, notificationHeight, screenWidth);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка позиционирования уведомления {Index}", index);
                throw;
            }
        }

        /// <summary>
        /// Рассчитывает позицию по вертикали с учетом реальной высоты уведомлений
        /// </summary>
        private double CalculateTopPosition(UIElement notification, int index)
        {
            var top = _config.TopMargin;

            // Если это первое уведомление, просто возвращаем отступ сверху
            if (index == 0)
                return top;

            // Для последующих уведомлений используем фиксированную высоту расширенного уведомления
            // Это обеспечивает стабильное позиционирование независимо от состояния текущего уведомления
            var expandedHeight = _config.ExpandedNotificationHeight;
            var spacing = _config.VerticalSpacing;

            // Рассчитываем позицию с учетом фиксированной высоты расширенного уведомления
            // Используем конфигурируемый отступ для компактного размещения
            var compactSpacing = Math.Max(10, _config.VerticalSpacing * 0.15); // Минимум 10px или 15% от VerticalSpacing
            top += (index * (expandedHeight + compactSpacing));

            // Проверяем, не выходит ли уведомление за пределы экрана
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var notificationHeight = expandedHeight;

            if (top + notificationHeight > screenHeight - 50) // 50px отступ снизу
            {
                // Если уведомление выходит за пределы, размещаем его выше
                top = screenHeight - notificationHeight - 50;
                _logger?.LogWarning("Уведомление {Index} размещено в нижней части экрана из-за нехватки места", index);
            }

            return top;
        }

        /// <summary>
        /// Пересчитывает позиции всех уведомлений с учетом их реальной высоты
        /// </summary>
        public void RecalculateAllPositions(IEnumerable<UIElement> notifications)
        {
            try
            {
                var notificationList = notifications.ToList();

                System.Diagnostics.Debug.WriteLine($"RecalculateAllPositions: пересчитываем позиции для {notificationList.Count} уведомлений");

                // Оптимизированное обновление layout - только один раз для всех уведомлений
                var frameworkElements = notificationList.OfType<FrameworkElement>().ToList();
                if (frameworkElements.Any())
                {
                    // Оптимизированно обновляем layout для всех элементов одновременно
                    foreach (var element in frameworkElements)
                    {
                        OptimizedLayoutUpdate(element);
                    }
                }

                for (int i = 0; i < notificationList.Count; i++)
                {
                    var notification = notificationList[i];
                    var newTop = CalculateTopPosition(notification, i);
                    var currentTop = Canvas.GetTop(notification);

                    System.Diagnostics.Debug.WriteLine($"  Уведомление {i}: текущая позиция={currentTop}, новая позиция={newTop}, разница={Math.Abs(currentTop - newTop)}");

                    // Анимируем только если позиция изменилась
                    if (Math.Abs(currentTop - newTop) > 1)
                    {
                        AnimateToPosition(notification, i);
                        System.Diagnostics.Debug.WriteLine($"  Уведомление {i}: анимируем перемещение");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  Уведомление {i}: позиция не изменилась, пропускаем");
                    }
                }

                _logger?.LogDebug("Пересчитаны позиции для {Count} уведомлений", notificationList.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка пересчета позиций уведомлений");
                throw;
            }
        }

        /// <summary>
        /// Анимированно перемещает уведомление в новую позицию
        /// </summary>
        public void AnimateToPosition(UIElement notification, int index)
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;

                // Используем реальную ширину уведомления для правильного центрирования
                var frameworkElement = notification as FrameworkElement;
                var notificationWidth = frameworkElement?.ActualWidth > 0 ? frameworkElement.ActualWidth : notification.DesiredSize.Width;

                // Если ширина не определена, используем минимальную ширину
                if (notificationWidth <= 0)
                {
                    notificationWidth = _config.MinNotificationWidth;
                }

                var left = (screenWidth - notificationWidth) / 2;
                var newTop = CalculateTopPosition(notification, index);

                // Получаем текущие позиции
                var currentTop = Canvas.GetTop(notification);
                var currentLeft = Canvas.GetLeft(notification);

                // Если позиция не изменилась, не анимируем
                if (Math.Abs(currentTop - newTop) < 1 && Math.Abs(currentLeft - left) < 1)
                    return;

                // Создаем анимацию движения по вертикали
                var topAnimation = new System.Windows.Media.Animation.DoubleAnimation(
                    currentTop,
                    newTop,
                    TimeSpan.FromMilliseconds(_config.RepositionAnimationDuration))
                {
                    EasingFunction = new System.Windows.Media.Animation.CubicEase
                    {
                        EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut
                    }
                };

                // Создаем анимацию движения по горизонтали (если изменилась)
                if (Math.Abs(currentLeft - left) >= 1)
                {
                    var leftAnimation = new System.Windows.Media.Animation.DoubleAnimation(
                        currentLeft,
                        left,
                        TimeSpan.FromMilliseconds(_config.RepositionAnimationDuration))
                    {
                        EasingFunction = new System.Windows.Media.Animation.CubicEase
                        {
                            EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut
                        }
                    };

                    // Применяем анимацию горизонтального движения
                    notification.BeginAnimation(Canvas.LeftProperty, leftAnimation);
                }

                // Применяем анимацию вертикального движения
                notification.BeginAnimation(Canvas.TopProperty, topAnimation);

                _logger?.LogDebug("Уведомление {Index} анимированно перемещено: {CurrentTop} -> {NewTop}",
                    index, currentTop, newTop);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка анимированного позиционирования уведомления {Index}", index);
                // В случае ошибки используем обычное позиционирование БЕЗ анимации
                try
                {
                    var screenWidth = SystemParameters.PrimaryScreenWidth;
                    var notificationWidth = GetNotificationWidth(notification);
                    var left = (screenWidth - notificationWidth) / 2;
                    var top = CalculateTopPosition(notification, index);

                    // Останавливаем все анимации перед установкой позиции
                    notification.BeginAnimation(Canvas.LeftProperty, null);
                    notification.BeginAnimation(Canvas.TopProperty, null);

                    Canvas.SetLeft(notification, left);
                    Canvas.SetTop(notification, top);

                    _logger?.LogDebug("Уведомление {Index} позиционировано без анимации после ошибки", index);
                }
                catch (Exception fallbackEx)
                {
                    _logger?.LogError(fallbackEx, "Критическая ошибка позиционирования уведомления {Index}", index);
                    // Устанавливаем позицию по умолчанию для предотвращения потери уведомления
                    try
                    {
                        Canvas.SetLeft(notification, 100);
                        Canvas.SetTop(notification, 100);
                    }
                    catch
                    {
                        // Последняя попытка - игнорируем ошибку
                    }
                }
            }
        }

        /// <summary>
        /// Пересчитывает позицию уведомления после изменения размера
        /// </summary>
        public void RecalculatePositionAfterResize(UIElement notification, int index)
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                // Оптимизированное обновление layout без задержек
                if (notification is FrameworkElement frameworkElement)
                {
                    OptimizedLayoutUpdate(frameworkElement);
                }

                // Используем реальную ширину уведомления для правильного центрирования
                var notificationWidth = GetNotificationWidth(notification);
                var notificationHeight = GetNotificationHeight(notification);
                var left = CalculateHorizontalPosition(screenWidth, notificationWidth);

                // Рассчитываем позицию с учетом реальной высоты уведомлений
                var top = CalculateTopPosition(notification, index);

                // Проверяем границы экрана
                if (top + notificationHeight > screenHeight - 50) // 50px отступ снизу
                {
                    top = Math.Max(0, screenHeight - notificationHeight - 50);
                    _logger?.LogWarning("Уведомление {Index} размещено в нижней части экрана из-за нехватки места", index);
                }

                Canvas.SetLeft(notification, left);
                Canvas.SetTop(notification, top);

                // Убеждаемся, что уведомление видимо
                notification.Visibility = Visibility.Visible;

                _logger?.LogDebug("Позиция уведомления {Index} пересчитана после изменения размера: Left={Left}, Top={Top}",
                    index, left, top);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка пересчета позиции уведомления {Index} после изменения размера", index);
            }
        }

        /// <summary>
        /// Получает ширину уведомления с fallback логикой
        /// </summary>
        private double GetNotificationWidth(UIElement notification)
        {
            if (notification is FrameworkElement frameworkElement)
            {
                // Сначала пробуем ActualWidth
                if (frameworkElement.ActualWidth > 0)
                    return frameworkElement.ActualWidth;

                // Затем DesiredSize.Width
                if (notification.DesiredSize.Width > 0)
                    return notification.DesiredSize.Width;

                // Если есть явно заданная ширина
                if (frameworkElement.Width > 0)
                    return frameworkElement.Width;
            }

            // Fallback на минимальную ширину
            return _config.MinNotificationWidth;
        }

        /// <summary>
        /// Получает высоту уведомления с fallback логикой
        /// </summary>
        private double GetNotificationHeight(UIElement notification)
        {
            if (notification is FrameworkElement frameworkElement)
            {
                // Сначала пробуем ActualHeight
                if (frameworkElement.ActualHeight > 0)
                    return frameworkElement.ActualHeight;

                // Затем DesiredSize.Height
                if (notification.DesiredSize.Height > 0)
                    return notification.DesiredSize.Height;

                // Если есть явно заданная высота
                if (frameworkElement.Height > 0)
                    return frameworkElement.Height;
            }

            // Fallback на минимальную высоту
            return _config.MinNotificationHeight;
        }

        /// <summary>
        /// Рассчитывает горизонтальную позицию с проверкой границ экрана
        /// </summary>
        private double CalculateHorizontalPosition(double screenWidth, double notificationWidth)
        {
            var left = (screenWidth - notificationWidth) / 2;

            // Проверяем, не выходит ли уведомление за левую границу
            if (left < 10) // 10px отступ слева
            {
                left = 10;
            }
            // Проверяем, не выходит ли уведомление за правую границу
            else if (left + notificationWidth > screenWidth - 10) // 10px отступ справа
            {
                left = screenWidth - notificationWidth - 10;
            }

            return left;
        }

        /// <summary>
        /// Оптимизированное обновление layout для элемента
        /// </summary>
        private void OptimizedLayoutUpdate(FrameworkElement element)
        {
            if (element == null) return;

            try
            {
                // Обновляем layout только если элемент видим и не анимируется
                if (element.Visibility == Visibility.Visible && element.IsLoaded)
                {
                    element.UpdateLayout();

                    // Используем разумные максимальные размеры вместо бесконечности
                    var maxSize = new Size(
                        Math.Min(SystemParameters.PrimaryScreenWidth * 2, 2000), // Максимум 2 экрана по ширине
                        Math.Min(SystemParameters.PrimaryScreenHeight * 2, 2000) // Максимум 2 экрана по высоте
                    );
                    element.Measure(maxSize);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Ошибка при обновлении layout элемента");
            }
        }
    }
}
