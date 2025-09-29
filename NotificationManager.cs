using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using DynaNoty.Events;
using DynaNoty.Services;
using DynaNoty.Configuration;
using Microsoft.Extensions.Logging;

namespace DynaNoty
{
    /// <summary>
    /// Менеджер уведомлений Dynamic Island с улучшенной архитектурой
    /// </summary>
    public class NotificationManager : INotificationService
    {
        private readonly INotificationWindow _notificationWindow;
        private DispatcherTimer _cleanupTimer;
        private readonly NotificationConfiguration _config;
        private readonly INotificationQueue _queue;
        private readonly INotificationDisplayManager _displayManager;
        private readonly INotificationLifecycleManager _lifecycleManager;
        private readonly IErrorHandler _errorHandler;
        private readonly IRateLimiter _rateLimiter;
        private readonly IPerformanceMonitor _performanceMonitor;
        private readonly ISystemNotificationService _systemNotificationService;
        private readonly ILogger<NotificationManager> _logger;
        private readonly object _queueProcessingLock = new object();
        private bool _disposed = false;

        public event EventHandler<NotificationDismissedEventArgs> NotificationDismissed
        {
            add => _displayManager.NotificationDismissed += value;
            remove => _displayManager.NotificationDismissed -= value;
        }

        public event EventHandler<Models.NotificationActionEventArgs> NotificationActionClicked
        {
            add => _displayManager.NotificationActionClicked += value;
            remove => _displayManager.NotificationActionClicked -= value;
        }

        public int ActiveNotificationCount => _lifecycleManager.ActiveNotificationCount;

        /// <summary>
        /// Получить статистику производительности
        /// </summary>
        public (int Active, int PoolSize, int TotalCreated, int ReuseRate) GetPerformanceStats()
        {
            // Временно возвращаем базовую статистику
            return (_lifecycleManager.ActiveNotificationCount, 0, 0, 0);
        }

        /// <summary>
        /// Создает NotificationManager с зависимостями
        /// </summary>
        public NotificationManager(NotificationManagerDependencies dependencies)
        {
            if (dependencies == null)
                throw new ArgumentNullException(nameof(dependencies));

            _config = dependencies.Config ?? new NotificationConfiguration();

            // Валидация конфигурации
            ValidateConfiguration(_config);
            _logger = dependencies.Logger;
            _queue = dependencies.Queue ?? new NotificationQueue(dependencies.LoggerFactory?.CreateLogger<NotificationQueue>());
            _errorHandler = dependencies.ErrorHandler ?? new ErrorHandler(dependencies.LoggerFactory?.CreateLogger<ErrorHandler>());
            _rateLimiter = dependencies.RateLimiter ?? new RateLimiter(
                _config,
                dependencies.LoggerFactory?.CreateLogger<RateLimiter>());
            _performanceMonitor = dependencies.PerformanceMonitor ?? new PerformanceMonitor(dependencies.LoggerFactory?.CreateLogger<PerformanceMonitor>());
            _systemNotificationService = dependencies.SystemNotificationService ?? new SystemNotificationService(dependencies.LoggerFactory?.CreateLogger<SystemNotificationService>());
            _notificationWindow = dependencies.NotificationWindow ?? new NotificationWindow(_config);
            _lifecycleManager = dependencies.LifecycleManager ?? CreateDefaultLifecycleManager(_notificationWindow, _config, dependencies.LoggerFactory);
            _displayManager = dependencies.DisplayManager ?? CreateDefaultDisplayManager(_lifecycleManager, dependencies.LoggerFactory);

            InitializeManager();
        }

        /// <summary>
        /// Создает NotificationManager с упрощенными параметрами (для обратной совместимости)
        /// </summary>
        public NotificationManager(
            INotificationWindow notificationWindow = null,
            NotificationConfiguration config = null,
            INotificationQueue queue = null,
            INotificationDisplayManager displayManager = null,
            INotificationLifecycleManager lifecycleManager = null,
            IErrorHandler errorHandler = null,
            IRateLimiter rateLimiter = null,
            IPerformanceMonitor performanceMonitor = null,
            ISystemNotificationService systemNotificationService = null,
            ILogger<NotificationManager> logger = null,
            ILoggerFactory loggerFactory = null)
            : this(new NotificationManagerDependencies
            {
                NotificationWindow = notificationWindow,
                Config = config,
                Queue = queue,
                DisplayManager = displayManager,
                LifecycleManager = lifecycleManager,
                ErrorHandler = errorHandler,
                RateLimiter = rateLimiter,
                PerformanceMonitor = performanceMonitor,
                SystemNotificationService = systemNotificationService,
                Logger = logger,
                LoggerFactory = loggerFactory
            })
        {
        }

        /// <summary>
        /// Инициализирует менеджер после создания всех зависимостей
        /// </summary>
        private void InitializeManager()
        {
            // НЕ показываем окно при инициализации - покажем только когда есть уведомления

            // Таймер для очистки завершенных уведомлений и обработки очереди
            _cleanupTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_config.CleanupIntervalSeconds)
            };
            _cleanupTimer.Tick += OnCleanupTimerTick;
            _cleanupTimer.Start();

            // Инициализируем системные уведомления
            InitializeSystemNotifications();

            _logger?.LogInformation("NotificationManager инициализирован с конфигурацией: AutoHide={AutoHide}s, MaxWidth={MaxWidth}px, SystemNotifications={SystemNotifications}",
                _config.AutoHideTimeoutSeconds, _config.MaxNotificationWidth, _config.EnableSystemNotifications);
        }

        public void ShowNotification(string title, string subtitle, string icon = "🔔", List<NotificationAction> actions = null)
        {
            ThrowIfDisposed();

            using (_performanceMonitor.StartTiming("ShowNotification"))
            {
                _errorHandler.ExecuteWithRetry(() =>
                {
                    if (!_rateLimiter.CanMakeRequest("notification"))
                    {
                        _logger?.LogWarning("Rate limit превышен для уведомления: {Title}", title);
                        _performanceMonitor.IncrementCounter("notifications_rate_limited");
                        return;
                    }

                    var notificationData = new NotificationData(title, subtitle, icon, NotificationType.Standard, actions);
                    _queue.Enqueue(notificationData);
                    _ = ProcessQueue();
                    _performanceMonitor.IncrementCounter("notifications_queued");
                }, "ShowNotification");
            }
        }

        public void ShowMusicNotification(string title, string subtitle, string artist)
        {
            ThrowIfDisposed();

            _errorHandler.ExecuteWithRetry(() =>
            {
                if (!_rateLimiter.CanMakeRequest("music"))
                {
                    _logger?.LogWarning("Rate limit превышен для музыкального уведомления: {Title}", title);
                    return;
                }

                var notificationData = NotificationData.CreateMusicNotification(title, subtitle, artist);
                _queue.Enqueue(notificationData);
                _ = ProcessQueue();
            }, "ShowMusicNotification");
        }

        public void ShowCallNotification(string title, string caller, string icon = "📞")
        {
            ThrowIfDisposed();

            _errorHandler.ExecuteWithRetry(() =>
            {
                if (!_rateLimiter.CanMakeRequest("call"))
                {
                    _logger?.LogWarning("Rate limit превышен для уведомления о звонке: {Title}", title);
                    return;
                }

                var notificationData = NotificationData.CreateCallNotification(title, caller, icon);
                _queue.Enqueue(notificationData);
                _ = ProcessQueue();
            }, "ShowCallNotification");
        }

        public void ShowCompactNotification(string icon = "🔔")
        {
            ThrowIfDisposed();

            _errorHandler.ExecuteWithRetry(() =>
            {
                if (!_rateLimiter.CanMakeRequest("compact"))
                {
                    _logger?.LogWarning("Rate limit превышен для компактного уведомления");
                    return;
                }

                var notificationData = NotificationData.CreateCompactNotification(icon);
                _queue.Enqueue(notificationData);
                _ = ProcessQueue();
            }, "ShowCompactNotification");
        }

        private async Task ShowNotificationInternalAsync(NotificationData notificationData)
        {
            // Показываем обычное уведомление
            await _displayManager.ShowNotificationAsync(notificationData, _config.MaxNotifications);

            // Показываем системное уведомление, если включено
            if (ShouldShowSystemNotification(notificationData))
            {
                try
                {
                    await _systemNotificationService.ShowNotificationAsync(notificationData);
                    _logger?.LogDebug("Показано системное уведомление: {Title}", notificationData.Title);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка показа системного уведомления: {Title}", notificationData.Title);
                }
            }
        }

        private async Task ProcessQueue()
        {
            try
            {
                // Быстрая проверка без блокировки
                if (_lifecycleManager.ActiveNotificationCount >= _config.MaxNotifications)
                    return;

                if (_queue.Count == 0)
                    return;

                NotificationData notificationData = null;

                // Минимальная блокировка только для извлечения из очереди
                lock (_queueProcessingLock)
                {
                    // Повторная проверка под блокировкой
                    if (_lifecycleManager.ActiveNotificationCount >= _config.MaxNotifications)
                        return;

                    if (_queue.Count > 0)
                    {
                        notificationData = _queue.Dequeue();
                    }
                }

                // Обрабатываем уведомление вне блокировки
                if (notificationData != null)
                {
                    try
                    {
                        await ShowNotificationInternalAsync(notificationData);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ошибка обработки уведомления из очереди: {Title}", notificationData.Title);

                        // Повторно добавляем в очередь для повторной попытки (с ограничением и задержкой)
                        if (notificationData.RetryCount < 3)
                        {
                            notificationData.IncrementRetryCount();

                            // Добавляем задержку перед повторной попыткой
                            var delay = TimeSpan.FromMilliseconds(100 * notificationData.RetryCount);
                            _ = Task.Delay(delay).ContinueWith(_ =>
                            {
                                _queue.Enqueue(notificationData);
                                _logger?.LogInformation("Уведомление возвращено в очередь для повторной попытки. Попытка: {RetryCount}, задержка: {Delay}ms",
                                    notificationData.RetryCount, delay.TotalMilliseconds);
                            }, TaskScheduler.Default);
                        }
                        else
                        {
                            _logger?.LogWarning("Уведомление {Title} удалено после 3 неудачных попыток", notificationData.Title);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Критическая ошибка в ProcessQueue");
                // Не перебрасываем исключение, чтобы не нарушить работу таймера
            }
        }

        private void OnCleanupTimerTick(object sender, EventArgs e)
        {
            try
            {
                // Проверяем, есть ли активные уведомления или очередь
                if (_lifecycleManager.ActiveNotificationCount == 0 && _queue.Count == 0)
                {
                    return; // Не выполняем очистку если нет работы
                }

                System.Diagnostics.Debug.WriteLine("OnCleanupTimerTick вызван");
                _lifecycleManager.CleanupCompletedNotifications();
                _ = ProcessQueue(); // Обрабатываем очередь при каждой очистке
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка в OnCleanupTimerTick");
            }
        }

        public void ClearAllNotifications()
        {
            ThrowIfDisposed();

            try
            {
                // Очищаем очередь
                _queue.Clear();

                // Закрываем все активные уведомления
                _lifecycleManager.ClearAllNotifications();

                _logger?.LogInformation("Очищены все уведомления и очередь");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка очистки всех уведомлений");
                throw;
            }
        }


        /// <summary>
        /// Валидирует конфигурацию уведомлений
        /// </summary>
        private void ValidateConfiguration(NotificationConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (config.MaxNotifications <= 0)
                throw new ArgumentException("MaxNotifications должно быть больше 0", nameof(config));

            if (config.AutoHideTimeoutSeconds <= 0)
                throw new ArgumentException("AutoHideTimeoutSeconds должно быть больше 0", nameof(config));

            if (config.MaxNotificationWidth <= 0)
                throw new ArgumentException("MaxNotificationWidth должно быть больше 0", nameof(config));

            if (config.MinNotificationWidth <= 0)
                throw new ArgumentException("MinNotificationWidth должно быть больше 0", nameof(config));

            if (config.MinNotificationWidth > config.MaxNotificationWidth)
                throw new ArgumentException("MinNotificationWidth не может быть больше MaxNotificationWidth", nameof(config));
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NotificationManager));
        }

        /// <summary>
        /// Инициализирует системные уведомления
        /// </summary>
        private void InitializeSystemNotifications()
        {
            if (!_config.EnableSystemNotifications)
            {
                _logger?.LogInformation("Системные уведомления отключены в конфигурации");
                return;
            }

            try
            {
                if (!_systemNotificationService.IsSupported())
                {
                    _logger?.LogWarning("Системные уведомления не поддерживаются на данной системе");
                    return;
                }

                // Подписываемся на события системных уведомлений
                _systemNotificationService.NotificationClicked += OnSystemNotificationClicked;
                _systemNotificationService.ActionClicked += OnSystemNotificationActionClicked;
                _systemNotificationService.NotificationDismissed += OnSystemNotificationDismissed;

                _logger?.LogInformation("Системные уведомления инициализированы");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка инициализации системных уведомлений");
            }
        }

        /// <summary>
        /// Определяет, нужно ли показывать системное уведомление
        /// </summary>
        private bool ShouldShowSystemNotification(NotificationData notificationData)
        {
            if (!_config.EnableSystemNotifications)
                return false;

            if (!_systemNotificationService.IsSupported())
                return false;

            if (!_systemNotificationService.HasPermission())
                return false;

            // Проверяем настройки фильтрации
            if (_config.SystemNotificationsForImportantOnly)
            {
                // Показываем только для важных типов уведомлений
                return notificationData.Type == NotificationType.Call;
            }

            if (!_config.SystemNotificationsForAllTypes)
            {
                // Показываем только для определенных типов
                return notificationData.Type == NotificationType.Call || notificationData.Type == NotificationType.Music;
            }

            return true;
        }

        /// <summary>
        /// Обработчик клика по системному уведомлению
        /// </summary>
        private void OnSystemNotificationClicked(object sender, SystemNotificationClickedEventArgs e)
        {
            _logger?.LogInformation("Клик по системному уведомлению: {NotificationId}", e.NotificationId);
            // Можно добавить дополнительную логику обработки клика
        }

        /// <summary>
        /// Обработчик клика по действию системного уведомления
        /// </summary>
        private void OnSystemNotificationActionClicked(object sender, SystemNotificationActionClickedEventArgs e)
        {
            _logger?.LogInformation("Клик по действию системного уведомления: {NotificationId}, Action: {ActionId}",
                e.NotificationId, e.ActionId);
            // Можно добавить дополнительную логику обработки действия
        }

        /// <summary>
        /// Обработчик отклонения системного уведомления
        /// </summary>
        private void OnSystemNotificationDismissed(object sender, SystemNotificationDismissedEventArgs e)
        {
            _logger?.LogInformation("Системное уведомление отклонено: {NotificationId}", e.NotificationId);
            // Можно добавить дополнительную логику обработки отклонения
        }

        private INotificationLifecycleManager CreateDefaultLifecycleManager(
            INotificationWindow notificationWindow,
            NotificationConfiguration config,
            ILoggerFactory loggerFactory)
        {
            var positioningService = new NotificationPositioningService(config,
                loggerFactory?.CreateLogger<NotificationPositioningService>());
            var pool = new NotificationPool(config,
                new SystemThemeService(loggerFactory?.CreateLogger<SystemThemeService>()),
                loggerFactory?.CreateLogger<NotificationPool>());

            return new NotificationLifecycleManager(
                notificationWindow ?? new NotificationWindow(config),
                positioningService,
                pool,
                loggerFactory?.CreateLogger<NotificationLifecycleManager>());
        }

        private INotificationDisplayManager CreateDefaultDisplayManager(INotificationLifecycleManager lifecycleManager, ILoggerFactory loggerFactory)
        {
            if (lifecycleManager == null)
                throw new ArgumentNullException(nameof(lifecycleManager));

            var validationService = new InputValidationService(
                _config,
                loggerFactory?.CreateLogger<InputValidationService>());

            // Используем пул из переданного lifecycleManager
            var pool = lifecycleManager.GetPool();

            var registry = new NotificationTypeHandlerRegistry(
                loggerFactory?.CreateLogger<NotificationTypeHandlerRegistry>());

            // Регистрируем обработчики
            registry.RegisterHandler(new Services.Handlers.CompactNotificationHandler());
            registry.RegisterHandler(new Services.Handlers.MusicNotificationHandler());
            registry.RegisterHandler(new Services.Handlers.CallNotificationHandler());
            registry.RegisterHandler(new Services.Handlers.StandardNotificationHandler());

            return new NotificationDisplayManager(
                pool,
                lifecycleManager,
                validationService,
                registry,
                loggerFactory?.CreateLogger<NotificationDisplayManager>());
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanupTimer?.Stop();
                if (_cleanupTimer != null)
                    _cleanupTimer.Tick -= OnCleanupTimerTick;

                ClearAllNotifications();

                _displayManager?.Dispose();
                _lifecycleManager?.Dispose();
                _errorHandler?.Dispose();
                _rateLimiter?.Dispose();
                _performanceMonitor?.Dispose();
                _systemNotificationService?.Dispose();
                _notificationWindow?.Dispose();

                _disposed = true;
                _logger?.LogInformation("NotificationManager освобожден");
            }
        }
    }
}

