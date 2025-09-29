using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using DynaNoty.Events;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для работы с системными уведомлениями Windows
    /// </summary>
    public class SystemNotificationService : ISystemNotificationService, IDisposable
    {
        private readonly ILogger<SystemNotificationService> _logger;
        private readonly ToastNotificationService _toastService;
        private bool _disposed = false;

        public event EventHandler<SystemNotificationClickedEventArgs> NotificationClicked;
        public event EventHandler<SystemNotificationActionClickedEventArgs> ActionClicked;
        public event EventHandler<SystemNotificationDismissedEventArgs> NotificationDismissed;

        public SystemNotificationService(ILogger<SystemNotificationService> logger = null)
        {
            _logger = logger;
            _toastService = new ToastNotificationService();

            // Подписываемся на события toast-сервиса
            _toastService.NotificationClicked += (s, e) => NotificationClicked?.Invoke(s, e);
            _toastService.ActionClicked += (s, e) => ActionClicked?.Invoke(s, e);
            _toastService.NotificationDismissed += (s, e) => NotificationDismissed?.Invoke(s, e);

            _logger?.LogInformation("SystemNotificationService инициализирован с ToastNotificationService");
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
                _logger?.LogWarning("Системные уведомления не поддерживаются");
                return Task.FromResult<string>(null);
            }

            try
            {
                return _toastService.ShowNotificationAsync(notificationData);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка показа системного уведомления: {Title}", notificationData.Title);
                return Task.FromResult<string>(null);
            }
        }

        public Task HideNotificationAsync(string notificationId)
        {
            ThrowIfDisposed();

            try
            {
                return _toastService.HideNotificationAsync(notificationId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка скрытия системного уведомления: {NotificationId}", notificationId);
                return Task.CompletedTask;
            }
        }

        public Task HideAllNotificationsAsync()
        {
            ThrowIfDisposed();

            try
            {
                return _toastService.HideAllNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка скрытия всех системных уведомлений");
                return Task.CompletedTask;
            }
        }

        public bool IsSupported()
        {
            try
            {
                return _toastService.IsSupported();
            }
            catch
            {
                return false;
            }
        }

        public Task<bool> RequestPermissionAsync()
        {
            ThrowIfDisposed();

            try
            {
                // В WPF разрешение не требуется
                return Task.FromResult(IsSupported());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка запроса разрешения на системные уведомления");
                return Task.FromResult(false);
            }
        }

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


        /// <summary>
        /// Обрабатывает активацию уведомления
        /// </summary>
        public void HandleNotificationActivated(object args)
        {
            try
            {
                // Заглушка для обработки активации уведомления
                _logger?.LogInformation("Обработка активации уведомления (заглушка)");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обработки активации системного уведомления");
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
                    _toastService.HandleNotificationDismissed(notificationId);
                    NotificationDismissed?.Invoke(this, new SystemNotificationDismissedEventArgs(notificationId, string.Empty, string.Empty));
                    _logger?.LogInformation("Системное уведомление отклонено: {NotificationId}", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обработки отклонения системного уведомления");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SystemNotificationService));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _toastService?.Dispose();
                    _disposed = true;
                    _logger?.LogInformation("SystemNotificationService освобожден");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка освобождения SystemNotificationService");
                }
            }
        }
    }
}
