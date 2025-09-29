using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using DynaNoty.Events;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Менеджер отображения уведомлений
    /// </summary>
    public class NotificationDisplayManager : INotificationDisplayManager
    {
        private readonly INotificationPool _pool;
        private readonly INotificationLifecycleManager _lifecycleManager;
        private readonly IInputValidationService _validationService;
        private readonly INotificationTypeHandlerRegistry _handlerRegistry;
        private readonly ILogger<NotificationDisplayManager> _logger;
        private bool _disposed = false;

        public event EventHandler<NotificationDismissedEventArgs> NotificationDismissed;
        public event EventHandler<Models.NotificationActionEventArgs> NotificationActionClicked;

        public NotificationDisplayManager(
            INotificationPool pool,
            INotificationLifecycleManager lifecycleManager,
            IInputValidationService validationService,
            INotificationTypeHandlerRegistry handlerRegistry,
            ILogger<NotificationDisplayManager> logger = null)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _lifecycleManager = lifecycleManager ?? throw new ArgumentNullException(nameof(lifecycleManager));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
            _logger = logger;
        }

        /// <summary>
        /// Показывает уведомление
        /// </summary>
        public async Task ShowNotificationAsync(NotificationData notificationData, int maxNotifications)
        {
            // Валидация входных параметров
            if (notificationData == null)
                throw new ArgumentNullException(nameof(notificationData));
            
            if (maxNotifications <= 0)
                throw new ArgumentException("Максимальное количество уведомлений должно быть больше 0", nameof(maxNotifications));

            ThrowIfDisposed();

            const int maxRetries = 3;
            var retryCount = 0;

            System.Diagnostics.Debug.WriteLine($"ShowNotificationAsync вызван: {notificationData.Type} - {notificationData.Title}");
            _logger?.LogInformation("Начинаем показ уведомления: {Type} - {Title}", notificationData.Type, notificationData.Title);

            while (retryCount < maxRetries)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Попытка {retryCount + 1} создания уведомления");
                    var notification = await CreateNotificationAsync(notificationData);
                    
                    System.Diagnostics.Debug.WriteLine("Уведомление создано, добавляем в жизненный цикл");
                    _lifecycleManager.AddNotification(notification, maxNotifications);
                    
                    System.Diagnostics.Debug.WriteLine("Уведомление успешно добавлено в жизненный цикл");
                    _logger?.LogInformation("Уведомление успешно показано (попытка {Attempt})", retryCount + 1);
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    System.Diagnostics.Debug.WriteLine($"Ошибка показа уведомления (попытка {retryCount}): {ex.Message}");
                    _logger?.LogWarning(ex, "Ошибка показа уведомления (попытка {Attempt}/{MaxRetries})", retryCount, maxRetries);
                    
                    if (retryCount >= maxRetries)
                    {
                        _logger?.LogError(ex, "Не удалось показать уведомление после {MaxRetries} попыток", maxRetries);
                        throw;
                    }
                    
                    // Задержка перед повторной попыткой с экспоненциальным backoff
                    var delay = Math.Min(1000, 100 * (int)Math.Pow(2, retryCount - 1));
                    await Task.Delay(delay);
                }
            }
        }

        /// <summary>
        /// Создает уведомление из пула
        /// </summary>
        private async Task<DynamicIslandNotification> CreateNotificationAsync(NotificationData notificationData)
        {
            System.Diagnostics.Debug.WriteLine("Получаем уведомление из пула");
            var notification = _pool.GetNotification();
            if (notification == null)
            {
                _logger?.LogError("Пул уведомлений вернул null");
                throw new InvalidOperationException("Не удалось получить уведомление из пула");
            }

            var notificationId = Guid.NewGuid().ToString();
            
            System.Diagnostics.Debug.WriteLine($"Создаем уведомление с ID: {notificationId} из пула");
            _logger?.LogDebug("Создаем уведомление с ID: {NotificationId} из пула", notificationId);
            
            // Создаем обработчики событий
            EventHandler dismissedHandler = null;
            EventHandler actionClickedHandler = null;

            try
            {
                System.Diagnostics.Debug.WriteLine("Создаем обработчики событий");
                dismissedHandler = (sender, e) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Уведомление {notificationId} закрыто");
                        _logger?.LogDebug("Уведомление {NotificationId} закрыто", notificationId);
                        NotificationDismissed?.Invoke(this, new NotificationDismissedEventArgs(notificationId));
                        
                        // Удаляем уведомление из жизненного цикла
                        _lifecycleManager.RemoveNotification(notification);
                    }
                    finally
                    {
                        // Всегда отписываемся от событий при закрытии
                        if (notification != null)
                        {
                            try { notification.Dismissed -= dismissedHandler; } catch { }
                            try { notification.ActionClicked -= actionClickedHandler; } catch { }
                        }
                    }
                };

                actionClickedHandler = (sender, e) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Действие нажато для уведомления {notificationId}");
                        _logger?.LogDebug("Действие нажато для уведомления {NotificationId}", notificationId);
                        NotificationActionClicked?.Invoke(this, new Models.NotificationActionEventArgs("dismiss", "Закрыть"));
                        notification?.Dismiss();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ошибка обработки действия для уведомления {NotificationId}", notificationId);
                    }
                };
                
                // Подписываемся на события
                notification.Dismissed += dismissedHandler;
                notification.ActionClicked += actionClickedHandler;

                // Получаем обработчик для типа уведомления и показываем уведомление
                // Это должно выполняться в UI потоке
                System.Diagnostics.Debug.WriteLine($"Получаем обработчик для типа: {notificationData.Type}");
                var handler = _handlerRegistry.GetHandler(notificationData.Type);
                
                // Получаем UI поток
                var dispatcher = GetUidispatcher();
                
                // Всегда используем асинхронный вызов для предотвращения deadlock
                System.Diagnostics.Debug.WriteLine("Выполняем в UI потоке асинхронно");
                await dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        // Валидация перед показом
                        if (notification == null)
                        {
                            _logger?.LogError("Уведомление равно null при показе");
                            throw new InvalidOperationException("Уведомление не может быть null");
                        }
                        
                        if (handler == null)
                        {
                            _logger?.LogError("Обработчик уведомления равен null");
                            throw new InvalidOperationException("Обработчик уведомления не найден");
                        }
                        
                        // Проверяем, что уведомление не удалено
                        if (notification.IsDisposed)
                        {
                            _logger?.LogWarning("Попытка показа удаленного уведомления");
                            return;
                        }
                        
                        handler.ShowNotification(notification, notificationData);
                    }
                    catch (InvalidOperationException)
                    {
                        // Перебрасываем критические ошибки
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ошибка показа уведомления в UI потоке: {Title}", notificationData.Title);
                        
                        // Пытаемся показать fallback уведомление
                        try
                        {
                            ShowFallbackNotification(notification, notificationData);
                        }
                        catch (Exception fallbackEx)
                        {
                            _logger?.LogError(fallbackEx, "Ошибка показа fallback уведомления");
                            throw new InvalidOperationException("Не удалось показать уведомление", ex);
                        }
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal);

                System.Diagnostics.Debug.WriteLine("Уведомление успешно создано и настроено");
                return notification;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания уведомления: {ex.Message}");
                // В случае ошибки отписываемся от событий и возвращаем уведомление в пул
                if (notification != null)
                {
                    try
                    {
                        // Безопасная отписка от событий
                        if (dismissedHandler != null)
                        {
                            try { notification.Dismissed -= dismissedHandler; } catch { }
                        }
                        if (actionClickedHandler != null)
                        {
                            try { notification.ActionClicked -= actionClickedHandler; } catch { }
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger?.LogWarning(cleanupEx, "Ошибка отписки от событий уведомления {NotificationId}", notificationId);
                    }
                    
                    try
                    {
                        _pool.ReturnNotification(notification);
                    }
                    catch (Exception poolEx)
                    {
                        _logger?.LogWarning(poolEx, "Ошибка возврата уведомления в пул {NotificationId}", notificationId);
                    }
                }
                
                _logger?.LogError(ex, "Ошибка создания уведомления {NotificationId}", notificationId);
                throw;
            }
        }

        /// <summary>
        /// Показывает fallback уведомление при ошибке
        /// </summary>
        private void ShowFallbackNotification(DynamicIslandNotification notification, NotificationData notificationData)
        {
            try
            {
                _logger?.LogInformation("Показываем fallback уведомление для: {Title}", notificationData.Title);
                
                // Простое уведомление без сложной логики
                notification.ShowNotification(
                    notificationData.Title ?? "Уведомление", 
                    notificationData.Subtitle ?? "", 
                    notificationData.Icon ?? "🔔", 
                    false, 
                    null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Критическая ошибка показа fallback уведомления");
                throw;
            }
        }

        /// <summary>
        /// Получает UI поток
        /// </summary>
        private System.Windows.Threading.Dispatcher GetUidispatcher()
        {
            // Используем Application.Current.Dispatcher как основной способ
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            
            if (dispatcher == null)
            {
                _logger?.LogError("UI поток недоступен для показа уведомления. Application.Current = {ApplicationCurrent}", 
                    System.Windows.Application.Current != null ? "not null" : "null");
                throw new InvalidOperationException("UI поток недоступен для показа уведомления. Проверьте, что приложение запущено в WPF контексте.");
            }
            
            return dispatcher;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NotificationDisplayManager));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _logger?.LogInformation("NotificationDisplayManager освобожден");
            }
        }
    }
}
