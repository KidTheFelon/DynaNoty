using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Асинхронная очередь уведомлений на основе Channel
    /// </summary>
    public class NotificationChannel : INotificationChannel
    {
        private readonly Channel<NotificationData> _channel;
        private readonly ChannelWriter<NotificationData> _writer;
        private readonly ChannelReader<NotificationData> _reader;
        private readonly ILogger<NotificationChannel> _logger;
        private bool _disposed = false;

        public NotificationChannel(ILogger<NotificationChannel> logger = null)
        {
            _logger = logger;
            
            var options = new BoundedChannelOptions(1000) // Максимум 1000 уведомлений в очереди
            {
                FullMode = BoundedChannelFullMode.Wait, // Ждем, если очередь полная
                SingleReader = false, // Может читать несколько читателей
                SingleWriter = false // Может писать несколько писателей
            };

            _channel = Channel.CreateBounded<NotificationData>(options);
            _writer = _channel.Writer;
            _reader = _channel.Reader;
        }

        /// <summary>
        /// Добавляет уведомление в канал
        /// </summary>
        public async Task<bool> WriteAsync(NotificationData notification, CancellationToken cancellationToken = default)
        {
            if (_disposed) return false;

            try
            {
                await _writer.WriteAsync(notification, cancellationToken);
                _logger?.LogDebug("Уведомление добавлено в канал. Тип: {Type}", notification.Type);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка записи уведомления в канал");
                return false;
            }
        }

        /// <summary>
        /// Читает уведомление из канала
        /// </summary>
        public async Task<NotificationData> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) return null;

            try
            {
                var notification = await _reader.ReadAsync(cancellationToken);
                _logger?.LogDebug("Уведомление прочитано из канала. Тип: {Type}", notification.Type);
                return notification;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogDebug("Чтение из канала отменено");
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка чтения уведомления из канала");
                return null;
            }
        }

        /// <summary>
        /// Пытается прочитать уведомление без блокировки
        /// </summary>
        public bool TryRead(out NotificationData notification)
        {
            notification = null;
            if (_disposed) return false;

            try
            {
                return _reader.TryRead(out notification);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка попытки чтения уведомления из канала");
                return false;
            }
        }

        /// <summary>
        /// Ожидает завершения канала
        /// </summary>
        public async Task WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) return;

            try
            {
                await _reader.WaitToReadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogDebug("Ожидание чтения из канала отменено");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка ожидания чтения из канала");
            }
        }

        /// <summary>
        /// Завершает канал
        /// </summary>
        public void Complete()
        {
            if (_disposed) return;

            try
            {
                _writer.Complete();
                _logger?.LogInformation("Канал уведомлений завершен");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка завершения канала");
            }
        }

        /// <summary>
        /// Завершает канал с ошибкой
        /// </summary>
        public void Complete(Exception exception)
        {
            if (_disposed) return;

            try
            {
                _writer.Complete(exception);
                _logger?.LogError(exception, "Канал уведомлений завершен с ошибкой");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка завершения канала с ошибкой");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Complete();
                _logger?.LogDebug("NotificationChannel освобожден");
            }
        }
    }
}
