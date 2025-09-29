using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;

namespace DynaNoty.Services
{
    /// <summary>
    /// Монитор производительности для отслеживания метрик
    /// </summary>
    public class PerformanceMonitor : IPerformanceMonitor
    {
        private readonly ILogger<PerformanceMonitor> _logger;
        private readonly ConcurrentDictionary<string, long> _counters = new();
        private readonly Timer _reportTimer;
        private bool _disposed = false;

        public PerformanceMonitor(ILogger<PerformanceMonitor> logger = null)
        {
            _logger = logger;
            
            // Отчет каждые 30 секунд
            _reportTimer = new Timer(ReportMetrics, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Начинает измерение времени выполнения операции
        /// </summary>
        public IDisposable StartTiming(string operationName)
        {
            var stopwatch = Stopwatch.StartNew();
            return new TimingScope(this, operationName, stopwatch);
        }

        /// <summary>
        /// Увеличивает счетчик
        /// </summary>
        public void IncrementCounter(string counterName, long value = 1)
        {
            _counters.AddOrUpdate(counterName, value, (key, existing) => existing + value);
        }

        /// <summary>
        /// Устанавливает значение счетчика
        /// </summary>
        public void SetCounter(string counterName, long value)
        {
            _counters.AddOrUpdate(counterName, value, (key, existing) => value);
        }

        /// <summary>
        /// Получает значение счетчика
        /// </summary>
        public long GetCounter(string counterName)
        {
            return _counters.TryGetValue(counterName, out var value) ? value : 0;
        }

        /// <summary>
        /// Получает все метрики
        /// </summary>
        public ConcurrentDictionary<string, long> GetAllMetrics()
        {
            return new ConcurrentDictionary<string, long>(_counters);
        }

        /// <summary>
        /// Очищает все метрики
        /// </summary>
        public void ClearMetrics()
        {
            _counters.Clear();
            _logger?.LogInformation("Метрики производительности очищены");
        }

        private void RecordTiming(string operationName, TimeSpan duration)
        {
            var counterName = $"{operationName}_duration_ms";
            IncrementCounter(counterName, (long)duration.TotalMilliseconds);
            
            var countName = $"{operationName}_count";
            IncrementCounter(countName);
            
            _logger?.LogDebug("Операция {OperationName} выполнена за {Duration}ms", operationName, duration.TotalMilliseconds);
        }

        private void ReportMetrics(object state)
        {
            if (_disposed) return;

            var metrics = GetAllMetrics();
            if (metrics.Count == 0) return;

            _logger?.LogInformation("=== Метрики производительности ===");
            foreach (var metric in metrics)
            {
                _logger?.LogInformation("{MetricName}: {Value}", metric.Key, metric.Value);
            }
            _logger?.LogInformation("================================");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _reportTimer?.Dispose();
                _logger?.LogDebug("PerformanceMonitor освобожден");
            }
        }

        private class TimingScope : IDisposable
        {
            private readonly PerformanceMonitor _monitor;
            private readonly string _operationName;
            private readonly Stopwatch _stopwatch;
            private bool _disposed = false;

            public TimingScope(PerformanceMonitor monitor, string operationName, Stopwatch stopwatch)
            {
                _monitor = monitor;
                _operationName = operationName;
                _stopwatch = stopwatch;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                    _monitor.RecordTiming(_operationName, _stopwatch.Elapsed);
                    _disposed = true;
                }
            }
        }
    }
}
