using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using DynaNoty.Events;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Менеджер жизненного цикла уведомлений
    /// </summary>
    public class NotificationLifecycleManager : INotificationLifecycleManager
    {
        private readonly List<DynamicIslandNotification> _activeNotifications = new();
        private readonly INotificationWindow _notificationWindow;
        private readonly INotificationPositioningService _positioningService;
        private readonly INotificationPool _pool;
        private readonly ILogger<NotificationLifecycleManager> _logger;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public event EventHandler<NotificationDismissedEventArgs> NotificationDismissed;
        public event EventHandler<Models.NotificationActionEventArgs> NotificationActionClicked;

        public int ActiveNotificationCount 
        { 
            get 
            { 
                lock (_lock) 
                { 
                    return _activeNotifications.Count; 
                } 
            } 
        }

        public NotificationLifecycleManager(
            INotificationWindow notificationWindow,
            INotificationPositioningService positioningService,
            INotificationPool pool,
            ILogger<NotificationLifecycleManager> logger = null)
        {
            _notificationWindow = notificationWindow ?? throw new ArgumentNullException(nameof(notificationWindow));
            _positioningService = positioningService ?? throw new ArgumentNullException(nameof(positioningService));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _logger = logger;
        }

        /// <summary>
        /// Добавляет уведомление в активный список
        /// </summary>
        public void AddNotification(DynamicIslandNotification notification, int maxNotifications)
        {
            ThrowIfDisposed();

            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            _logger?.LogDebug("Добавляем уведомление в жизненный цикл. MaxNotifications: {MaxNotifications}", maxNotifications);

            var addResult = PrepareNotificationAddition(notification, maxNotifications);
            
            try
            {
                AddNotificationToUI(notification, addResult.NotificationIndex, addResult.ShouldShowWindow);
                PositionAndValidateNotification(notification, addResult.NotificationIndex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка добавления уведомления в UI");
                RemoveNotificationFromList(notification);
                throw;
            }
            
            CleanupOldNotifications(addResult.ToRemove);
        }

        /// <summary>
        /// Подготавливает добавление уведомления
        /// </summary>
        private (List<DynamicIslandNotification> ToRemove, bool ShouldShowWindow, int NotificationIndex) PrepareNotificationAddition(
            DynamicIslandNotification notification, int maxNotifications)
        {
            List<DynamicIslandNotification> toRemove = null;
            bool shouldShowWindow = false;
            int notificationIndex = 0;
            
            lock (_lock)
            {
                // Очищаем старые уведомления, если их слишком много
                if (_activeNotifications.Count >= maxNotifications)
                {
                    toRemove = _activeNotifications.Take(_activeNotifications.Count - maxNotifications + 1).ToList();
                    
                    // Удаляем из списка активных уведомлений
                    foreach (var oldNotification in toRemove)
                    {
                        _activeNotifications.Remove(oldNotification);
                    }
                    
                    _logger?.LogDebug("Удалено {Count} старых уведомлений для освобождения места", toRemove.Count);
                }

                _activeNotifications.Add(notification);
                notificationIndex = _activeNotifications.Count - 1;
                
                // Показываем окно, если это первое уведомление
                if (_activeNotifications.Count == 1)
                {
                    shouldShowWindow = true;
                }
                
                _logger?.LogDebug("Уведомление добавлено. Всего активных: {Count}", _activeNotifications.Count);
            }

            return (toRemove, shouldShowWindow, notificationIndex);
        }

        /// <summary>
        /// Добавляет уведомление в UI
        /// </summary>
        private void AddNotificationToUI(DynamicIslandNotification notification, int notificationIndex, bool shouldShowWindow)
        {
            _notificationWindow.Container.Children.Add(notification);
            
            if (shouldShowWindow)
            {
                _notificationWindow.Show();
                _logger?.LogDebug("NotificationWindow показан при добавлении первого уведомления");
            }
            
            _logger?.LogDebug("Уведомление добавлено в контейнер. Детей в контейнере: {Count}", _notificationWindow.Container.Children.Count);
            
            // Убеждаемся, что уведомление видимо
            notification.Visibility = Visibility.Visible;
            notification.Opacity = 1.0;
        }

        /// <summary>
        /// Позиционирует и валидирует уведомление
        /// </summary>
        private void PositionAndValidateNotification(DynamicIslandNotification notification, int notificationIndex)
        {
            // Устанавливаем позиционирующий сервис для уведомления
            if (notification is DynamicIslandNotification dynamicNotification)
            {
                dynamicNotification.SetPositioningService(_positioningService, notificationIndex);
            }
            
            // Позиционируем уведомление после полной загрузки
            // Используем флаг для предотвращения множественных вызовов
            var isPositioned = false;
            notification.Loaded += (s, e) => 
            {
                if (isPositioned) return;
                isPositioned = true;
                
                try
                {
                    // Принудительно обновляем layout для получения актуальных размеров
                    notification.UpdateLayout();
                    
                    // Позиционируем новое уведомление
                    _positioningService.PositionNotification(notification, notificationIndex);
                    
                    // Пересчитываем позиции всех уведомлений для правильного размещения
                    if (_activeNotifications.Count > 1)
                    {
                        _positioningService.RecalculateAllPositions(_activeNotifications);
                        _logger?.LogDebug("Пересчитаны позиции всех уведомлений");
                    }
                    
                    // Дополнительная проверка видимости всех уведомлений
                    ValidateNotificationsVisibility();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка позиционирования уведомления {Index} в обработчике Loaded", notificationIndex);
                }
            };
            
            // Если уведомление уже загружено, позиционируем сразу
            if (notification.IsLoaded)
            {
                notification.UpdateLayout();
                _positioningService.PositionNotification(notification, notificationIndex);
                
                if (_activeNotifications.Count > 1)
                {
                    _positioningService.RecalculateAllPositions(_activeNotifications);
                    _logger?.LogDebug("Пересчитаны позиции всех уведомлений");
                }
                
                ValidateNotificationsVisibility();
            }
        }

        /// <summary>
        /// Валидирует видимость уведомлений
        /// </summary>
        private void ValidateNotificationsVisibility()
        {
            _logger?.LogDebug("Проверка видимости уведомлений");
            for (int i = 0; i < _activeNotifications.Count; i++)
            {
                var notif = _activeNotifications[i];
                var top = System.Windows.Controls.Canvas.GetTop(notif);
                var left = System.Windows.Controls.Canvas.GetLeft(notif);
                _logger?.LogDebug("Уведомление {Index}: Visibility={Visibility}, Opacity={Opacity}, Top={Top}, Left={Left}", 
                    i, notif.Visibility, notif.Opacity, top, left);
            }
        }

        /// <summary>
        /// Удаляет уведомление из списка
        /// </summary>
        private void RemoveNotificationFromList(DynamicIslandNotification notification)
        {
            lock (_lock)
            {
                _activeNotifications.Remove(notification);
            }
        }

        /// <summary>
        /// Очищает старые уведомления
        /// </summary>
        private void CleanupOldNotifications(List<DynamicIslandNotification> toRemove)
        {
            if (toRemove != null && toRemove.Count > 0)
            {
                foreach (var oldNotification in toRemove)
                {
                    try
                    {
                        _notificationWindow.Container.Children.Remove(oldNotification);
                        _pool.ReturnNotification(oldNotification);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ошибка удаления старого уведомления");
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет уведомление из активного списка
        /// </summary>
        public void RemoveNotification(DynamicIslandNotification notification)
        {
            if (notification == null) return;

            _logger?.LogDebug("RemoveNotification вызван");

            lock (_lock)
            {
                if (!_activeNotifications.Remove(notification))
                {
                    _logger?.LogWarning("Попытка удаления несуществующего уведомления");
                    return;
                }

                _notificationWindow.Container.Children.Remove(notification);
                
                // Возвращаем в пул для переиспользования
                _pool.ReturnNotification(notification);
                
                // Перепозиционируем оставшиеся уведомления
                RepositionRemainingNotifications();
                
                // Скрываем окно, если не осталось уведомлений
                if (_activeNotifications.Count == 0)
                {
                    _notificationWindow.Hide();
                    _logger?.LogDebug("NotificationWindow скрыт - нет активных уведомлений");
                }
                
                _logger?.LogDebug("Уведомление удалено. Осталось активных: {Count}", _activeNotifications.Count);
            }
        }

        /// <summary>
        /// Очищает все активные уведомления
        /// </summary>
        public void ClearAllNotifications()
        {
            ThrowIfDisposed();

            List<DynamicIslandNotification> notificationsToRemove;
            
            lock (_lock)
            {
                notificationsToRemove = _activeNotifications.ToList();
                _activeNotifications.Clear();
            }

            // Очищаем уведомления вне блокировки
            foreach (var notification in notificationsToRemove)
            {
                try
                {
                    _notificationWindow.Container.Children.Remove(notification);
                    notification.Dismiss();
                    _pool.ReturnNotification(notification);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка очистки уведомления");
                }
            }

            _notificationWindow.Hide();
            _logger?.LogInformation("Очищены все активные уведомления. Количество: {Count}", notificationsToRemove.Count);
        }

        /// <summary>
        /// Очищает завершенные уведомления
        /// </summary>
        public void CleanupCompletedNotifications()
        {
            try
            {
                List<DynamicIslandNotification> toRemove;
                
                lock (_lock)
                {
                    // Ищем только действительно завершенные уведомления:
                    // - Скрытые (Visibility != Visible) - это главный критерий
                    // - Или с нулевой прозрачностью И невидимые (для завершенных анимаций)
                    // НЕ удаляем уведомления с Opacity=0 если они Visible (это нормально для анимаций)
                    toRemove = _activeNotifications
                        .Where(n => n.Visibility != Visibility.Visible || 
                                   (n.Opacity <= 0 && n.Visibility == Visibility.Hidden))
                        .ToList();
                }

                if (toRemove.Count > 0)
                {
                    _logger?.LogDebug("CleanupCompletedNotifications нашел {Count} уведомлений для удаления", toRemove.Count);
                    foreach (var notification in toRemove)
                    {
                        _logger?.LogDebug("Удаляем уведомление: Visibility={Visibility}, Opacity={Opacity}", 
                            notification.Visibility, notification.Opacity);
                        RemoveNotification(notification);
                    }
                    _logger?.LogDebug("Очищено завершенных уведомлений: {Count}", toRemove.Count);
                }
                else
                {
                    // Логируем состояние всех активных уведомлений для отладки
                    lock (_lock)
                    {
                        foreach (var notification in _activeNotifications)
                        {
                            _logger?.LogDebug("Активное уведомление: Visibility={Visibility}, Opacity={Opacity}", 
                                notification.Visibility, notification.Opacity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка очистки завершенных уведомлений");
            }
        }

        /// <summary>
        /// Получает пул уведомлений
        /// </summary>
        public INotificationPool GetPool()
        {
            return _pool;
        }

        /// <summary>
        /// Перепозиционирует оставшиеся уведомления
        /// </summary>
        private void RepositionRemainingNotifications()
        {
            try
            {
                // Используем ConcurrentBag для безопасного доступа без блокировок
                var notificationsToReposition = new System.Collections.Concurrent.ConcurrentBag<DynamicIslandNotification>();
                
                // Быстрое копирование без длительной блокировки
                lock (_lock)
                {
                    foreach (var notification in _activeNotifications)
                    {
                        notificationsToReposition.Add(notification);
                    }
                }
                
                // Перепозиционируем уведомления параллельно
                var repositionTasks = new List<Task>();
                int index = 0;
                
                foreach (var notification in notificationsToReposition)
                {
                    var currentIndex = index++;
                    var currentNotification = notification;
                    
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            // Проверяем, что уведомление все еще активно
                            bool isStillActive;
                            lock (_lock)
                            {
                                isStillActive = _activeNotifications.Contains(currentNotification);
                            }
                            
                            if (isStillActive)
                            {
                                // Выполняем перепозиционирование в UI потоке
                                if (currentNotification.Dispatcher?.CheckAccess() == true)
                                {
                                    _positioningService.AnimateToPosition(currentNotification, currentIndex);
                                }
                                else
                                {
                                    currentNotification.Dispatcher?.BeginInvoke(() =>
                                    {
                                        try
                                        {
                                            _positioningService.AnimateToPosition(currentNotification, currentIndex);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger?.LogWarning(ex, "Ошибка перепозиционирования уведомления {Index} в UI потоке", currentIndex);
                                        }
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Ошибка перепозиционирования уведомления {Index}", currentIndex);
                        }
                    });
                    
                    repositionTasks.Add(task);
                }
                
                // Ждем завершения всех задач с таймаутом
                Task.WaitAll(repositionTasks.ToArray(), TimeSpan.FromSeconds(5));
                
                _logger?.LogDebug("Перепозиционированы {Count} уведомлений", notificationsToReposition.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка перепозиционирования уведомлений");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NotificationLifecycleManager));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                ClearAllNotifications();
                _disposed = true;
                _logger?.LogInformation("NotificationLifecycleManager освобожден");
            }
        }
    }
}
