using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Controls;
using DynaNoty.Configuration;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;

namespace DynaNoty.Services
{
    /// <summary>
    /// Оптимизированный пул объектов для переиспользования уведомлений
    /// </summary>
    public class NotificationPool : INotificationPool
    {
        private readonly ConcurrentQueue<DynamicIslandNotification> _pool = new();
        private readonly NotificationConfiguration _config;
        private readonly ISystemThemeService _themeService;
        private readonly ILogger<NotificationPool> _logger;
        private readonly object _lock = new object();
        private bool _disposed = false;
        private int _maxPoolSize = 10;
        private int _preWarmCount = 3;
        private int _createdCount = 0;

        public NotificationPool(NotificationConfiguration config, ISystemThemeService themeService = null, ILogger<NotificationPool> logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _themeService = themeService ?? new SystemThemeService();
            _logger = logger;

            // Включаем предварительное создание для лучшей производительности
            PreWarmPool();
        }

        /// <summary>
        /// Предварительное создание уведомлений для улучшения производительности
        /// </summary>
        private void PreWarmPool()
        {
            for (int i = 0; i < _preWarmCount; i++)
            {
                var notification = CreateOptimizedNotification();
                _pool.Enqueue(notification);
                _createdCount++;
            }
            _logger?.LogDebug("Предварительно создано {Count} уведомлений", _preWarmCount);
        }

        /// <summary>
        /// Создать оптимизированное уведомление
        /// </summary>
        private DynamicIslandNotification CreateOptimizedNotification()
        {
            // Создаем логгер для DynamicIslandNotification из логгера NotificationPool
            var notificationLogger = _logger != null ?
                Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<DynamicIslandNotification>() :
                null;

            var notification = new DynamicIslandNotification(_config, _themeService, notificationLogger);

            if (notification == null)
            {
                _logger?.LogError("Не удалось создать уведомление - конструктор вернул null");
                throw new InvalidOperationException("Ошибка создания уведомления");
            }

            // Предварительно настраиваем для лучшей производительности
            notification.Visibility = System.Windows.Visibility.Collapsed;
            notification.Opacity = 0;

            return notification;
        }

        /// <summary>
        /// Получает уведомление из пула или создает новое
        /// </summary>
        public DynamicIslandNotification GetNotification()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NotificationPool));

            if (_pool.TryDequeue(out var notification))
            {
                _logger?.LogDebug("Уведомление получено из пула. Осталось в пуле: {Count}", _pool.Count);
                return notification;
            }

            // Создаем новое уведомление с оптимизацией
            try
            {
                notification = CreateOptimizedNotification();
                _createdCount++;
                _logger?.LogDebug("Создано новое уведомление (всего создано: {Count})", _createdCount);
                return notification;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка создания нового уведомления");
                throw;
            }
        }

        /// <summary>
        /// Возвращает уведомление в пул для переиспользования
        /// </summary>
        public void ReturnNotification(DynamicIslandNotification notification)
        {
            if (_disposed || notification == null)
                return;

            System.Diagnostics.Debug.WriteLine("ReturnNotification вызван");

            try
            {
                // Сбрасываем состояние уведомления
                ResetNotification(notification);

                if (_pool.Count < _maxPoolSize)
                {
                    _pool.Enqueue(notification);
                    System.Diagnostics.Debug.WriteLine($"Уведомление возвращено в пул. Размер пула: {_pool.Count}");
                    _logger?.LogDebug("Уведомление возвращено в пул. Размер пула: {Count}", _pool.Count);
                }
                else
                {
                    // Пул переполнен, освобождаем уведомление
                    System.Diagnostics.Debug.WriteLine("Пул переполнен, освобождаем уведомление");
                    notification.Dispose();
                    _logger?.LogDebug("Пул переполнен, уведомление освобождено");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка возврата уведомления в пул: {ex.Message}");
                _logger?.LogError(ex, "Ошибка возврата уведомления в пул");
                notification?.Dispose();
            }
        }

        /// <summary>
        /// Сбрасывает состояние уведомления для переиспользования
        /// </summary>
        private void ResetNotification(DynamicIslandNotification notification)
        {
            if (notification == null)
            {
                _logger?.LogWarning("Уведомление равно null при сбросе");
                return;
            }

            if (_disposed)
            {
                _logger?.LogWarning("Попытка сброса уведомления после освобождения пула");
                return;
            }

            try
            {
                // Проверяем, что уведомление не удалено
                if (notification.IsDisposed)
                {
                    _logger?.LogWarning("Попытка сброса удаленного уведомления");
                    return;
                }

                // Получаем UI поток с упрощенной логикой
                var dispatcher = GetDispatcher(notification);

                if (dispatcher?.CheckAccess() == true)
                {
                    PerformReset(notification);
                }
                else if (dispatcher != null)
                {
                    // Используем BeginInvoke для асинхронного сброса
                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            PerformReset(notification);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Ошибка асинхронного сброса уведомления");
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
                else
                {
                    // Fallback - выполняем сброс синхронно
                    _logger?.LogWarning("UI поток недоступен, выполняем сброс синхронно");
                    PerformReset(notification);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при сбросе уведомления");
                // Последняя попытка сброса без UI потока
                try
                {
                    PerformReset(notification);
                }
                catch (Exception resetEx)
                {
                    _logger?.LogError(resetEx, "Критическая ошибка при сбросе уведомления");
                }
            }
        }

        /// <summary>
        /// Получает UI поток с упрощенной логикой
        /// </summary>
        private System.Windows.Threading.Dispatcher GetDispatcher(DynamicIslandNotification notification)
        {
            if (notification?.Dispatcher != null)
                return notification.Dispatcher;

            var appDispatcher = System.Windows.Application.Current?.Dispatcher;
            if (appDispatcher != null)
                return appDispatcher;

            _logger?.LogWarning("UI поток недоступен для уведомления");
            return null;
        }

        /// <summary>
        /// Выполняет фактический сброс состояния уведомления
        /// </summary>
        private void PerformReset(DynamicIslandNotification notification)
        {
            try
            {
                // Удаляем уведомление из родительского контейнера
                if (notification.Parent is Panel parentPanel)
                {
                    parentPanel.Children.Remove(notification);
                    _logger?.LogDebug("Уведомление удалено из родительского контейнера");
                }

                // Сбрасываем видимость и позицию
                notification.Opacity = 0;
                notification.Visibility = System.Windows.Visibility.Collapsed;

                // Сбрасываем размер
                notification.Width = double.NaN;
                notification.Height = double.NaN;

                _logger?.LogDebug("Состояние уведомления успешно сброшено");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка сброса состояния уведомления");
                throw;
            }
        }

        /// <summary>
        /// Очищает пул
        /// </summary>
        public void Clear()
        {
            if (_disposed)
                return;

            lock (_lock)
            {
                while (_pool.TryDequeue(out var notification))
                {
                    notification?.Dispose();
                }
                _logger?.LogInformation("Пул уведомлений очищен");
            }
        }

        /// <summary>
        /// Количество уведомлений в пуле
        /// </summary>
        public int Count => _pool.Count;

        /// <summary>
        /// Общее количество созданных уведомлений
        /// </summary>
        public int TotalCreated => _createdCount;

        /// <summary>
        /// Статистика производительности пула
        /// </summary>
        public (int PoolSize, int TotalCreated, int ReuseRate) GetPerformanceStats()
        {
            var reuseRate = _createdCount > 0 ? (int)((double)(_createdCount - _pool.Count) / _createdCount * 100) : 0;
            return (_pool.Count, _createdCount, reuseRate);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Clear();
                _logger?.LogInformation("NotificationPool освобожден");
            }
        }
    }
}
