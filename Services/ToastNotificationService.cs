using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using Hardcodet.Wpf.TaskbarNotification;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using DynaNoty.Events;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для работы с toast-уведомлениями WPF
    /// </summary>
    public class ToastNotificationService : ISystemNotificationService, IDisposable
    {
        private readonly ILogger<ToastNotificationService> _logger;
        private readonly TaskbarIcon _taskbarIcon;
        private readonly Dictionary<string, BalloonIcon> _activeNotifications;
        private bool _disposed = false;

        public event EventHandler<SystemNotificationClickedEventArgs> NotificationClicked;
        public event EventHandler<SystemNotificationActionClickedEventArgs> ActionClicked;
        public event EventHandler<SystemNotificationDismissedEventArgs> NotificationDismissed;

        public ToastNotificationService(ILogger<ToastNotificationService> logger = null)
        {
            _logger = logger;
            _activeNotifications = new Dictionary<string, BalloonIcon>();
            _taskbarIcon = new TaskbarIcon();
            
            _logger?.LogInformation("ToastNotificationService инициализирован");
        }

        public async Task<string> ShowNotificationAsync(string title, string body, string icon = null, List<NotificationAction> actions = null)
        {
            var notificationData = new NotificationData(title, body, icon, NotificationType.Standard, actions);
            return await ShowNotificationAsync(notificationData);
        }

        public Task<string> ShowNotificationAsync(NotificationData notificationData)
        {
            ThrowIfDisposed();

            if (!IsSupported())
            {
                _logger?.LogWarning("Toast-уведомления не поддерживаются");
                return Task.FromResult<string>(null);
            }

            try
            {
                var balloonIcon = GetBalloonIcon(notificationData.Type);
                
                // Добавляем в словарь активных уведомлений
                _activeNotifications[notificationData.Id] = balloonIcon;

                // Показываем уведомление
                _taskbarIcon.ShowBalloonTip(notificationData.Title, notificationData.Subtitle, balloonIcon);
                
                _logger?.LogInformation("Показано toast-уведомление: {Title}", notificationData.Title);
                return Task.FromResult(notificationData.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка показа toast-уведомления: {Title}", notificationData.Title);
                return Task.FromResult<string>(null);
            }
        }

        public Task HideNotificationAsync(string notificationId)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(notificationId))
                return Task.CompletedTask;

            try
            {
                if (_activeNotifications.ContainsKey(notificationId))
                {
                    // В Notifications.Wpf нет прямого метода скрытия по ID
                    // Уведомления скрываются автоматически по таймауту
                    _activeNotifications.Remove(notificationId);
                    _logger?.LogInformation("Скрыто toast-уведомление: {NotificationId}", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка скрытия toast-уведомления: {NotificationId}", notificationId);
            }

            return Task.CompletedTask;
        }

        public Task HideAllNotificationsAsync()
        {
            ThrowIfDisposed();

            try
            {
                _activeNotifications.Clear();
                _logger?.LogInformation("Скрыты все toast-уведомления");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка скрытия всех toast-уведомлений");
            }

            return Task.CompletedTask;
        }

        public bool IsSupported()
        {
            try
            {
                return true; // Notifications.Wpf всегда поддерживается
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка проверки поддержки toast-уведомлений");
                return false;
            }
        }

        /// <summary>
        /// Преобразует тип уведомления в BalloonIcon
        /// </summary>
        private BalloonIcon GetBalloonIcon(Models.NotificationType notificationType)
        {
            return notificationType switch
            {
                Models.NotificationType.Standard => BalloonIcon.Info,
                Models.NotificationType.Music => BalloonIcon.Info,
                Models.NotificationType.Call => BalloonIcon.Info,
                Models.NotificationType.Compact => BalloonIcon.Info,
                _ => BalloonIcon.Info
            };
        }

        /// <summary>
        /// Обрабатывает клик по уведомлению
        /// </summary>
        private void OnNotificationClicked(string notificationId)
        {
            try
            {
                NotificationClicked?.Invoke(this, new SystemNotificationClickedEventArgs(notificationId, string.Empty, string.Empty));
                _logger?.LogInformation("Клик по toast-уведомлению: {NotificationId}", notificationId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обработки клика по toast-уведомлению");
            }
        }

        /// <summary>
        /// Обрабатывает клик по действию
        /// </summary>
        private void OnActionClicked(string notificationId, string actionId, string actionText)
        {
            try
            {
                ActionClicked?.Invoke(this, new SystemNotificationActionClickedEventArgs(notificationId, actionId, actionText));
                _logger?.LogInformation("Клик по действию toast-уведомления: {NotificationId}, Action: {Action}", notificationId, actionId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обработки действия toast-уведомления");
            }
        }

        /// <summary>
        /// Обрабатывает отклонение уведомления
        /// </summary>
        public void HandleNotificationDismissed(string notificationId)
        {
            try
            {
                if (!string.IsNullOrEmpty(notificationId))
                {
                    _activeNotifications.Remove(notificationId);
                    NotificationDismissed?.Invoke(this, new SystemNotificationDismissedEventArgs(notificationId, string.Empty, string.Empty));
                    _logger?.LogInformation("Toast-уведомление отклонено: {NotificationId}", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обработки отклонения toast-уведомления");
            }
        }

        /// <summary>
        /// Обрабатывает активацию уведомления (заглушка для совместимости)
        /// </summary>
        public void HandleNotificationActivated(object args)
        {
            // Заглушка для совместимости с интерфейсом
            _logger?.LogInformation("Обработка активации toast-уведомления (заглушка)");
        }

        /// <summary>
        /// Запрашивает разрешение на показ уведомлений
        /// </summary>
        public Task<bool> RequestPermissionAsync()
        {
            try
            {
                // В WPF разрешение не требуется
                return Task.FromResult(IsSupported());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка запроса разрешения на toast-уведомления");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Проверяет наличие разрешения на показ уведомлений
        /// </summary>
        public bool HasPermission()
        {
            try
            {
                return IsSupported();
            }
            catch
            {
                return false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ToastNotificationService));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _activeNotifications.Clear();
                    _taskbarIcon?.Dispose();
                    _disposed = true;
                    _logger?.LogInformation("ToastNotificationService освобожден");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка освобождения ToastNotificationService");
                }
            }
        }
    }
}
