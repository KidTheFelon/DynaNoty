using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DynaNoty.Configuration;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Менеджер состояний уведомления
    /// </summary>
    public class NotificationStateManager
    {
        private readonly NotificationConfiguration _config;
        private readonly ILogger _logger;
        private DispatcherTimer _autoHideTimer;
        private DispatcherTimer _expandTimer;
        private bool _disposed = false;

        public event EventHandler StateChanged;
        public event EventHandler AutoHideTriggered;

        public NotificationState CurrentState { get; private set; } = NotificationState.Compact;

        public NotificationStateManager(NotificationConfiguration config, ILogger logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
        }

        /// <summary>
        /// Изменяет состояние уведомления
        /// </summary>
        public void ChangeState(NotificationState newState)
        {
            if (_disposed || CurrentState == newState) return;

            var oldState = CurrentState;
            CurrentState = newState;
            
            _logger?.LogDebug("Состояние уведомления изменено: {OldState} -> {NewState}", oldState, newState);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Запускает таймер расширения
        /// </summary>
        public void StartExpandTimer()
        {
            if (_disposed || !_config.EnableAutoExpand) return;

            StopExpandTimer();

            _expandTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_config.ExpandDelay)
            };
            _expandTimer.Tick += OnExpandTimerTick;
            _expandTimer.Start();
            
            System.Diagnostics.Debug.WriteLine($"Таймер расширения запущен с интервалом {_config.ExpandDelay}ms");
            _logger?.LogInformation("Таймер расширения запущен с интервалом {Delay}ms", _config.ExpandDelay);
        }

        /// <summary>
        /// Останавливает таймер расширения
        /// </summary>
        public void StopExpandTimer()
        {
            if (_expandTimer != null)
            {
                _expandTimer.Stop();
                _expandTimer.Tick -= OnExpandTimerTick;
                _expandTimer = null;
            }
        }

        /// <summary>
        /// Запускает таймер автоскрытия
        /// </summary>
        public void StartAutoHideTimer()
        {
            if (_disposed) return;
            
            StopAutoHideTimer();
            
            var displayDuration = GetDisplayDuration();
            
            _autoHideTimer = new DispatcherTimer 
            { 
                Interval = TimeSpan.FromMilliseconds(displayDuration) 
            };
            _autoHideTimer.Tick += OnAutoHideTimerTick;
            _autoHideTimer.Start();
            
            System.Diagnostics.Debug.WriteLine($"Таймер автоскрытия запущен с интервалом {displayDuration}мс (состояние: {CurrentState})");
            _logger?.LogInformation("Таймер автоскрытия запущен с интервалом {Duration}мс (состояние: {State})", displayDuration, CurrentState);
        }

        /// <summary>
        /// Останавливает таймер автоскрытия
        /// </summary>
        public void StopAutoHideTimer()
        {
            if (_autoHideTimer != null)
            {
                _autoHideTimer.Stop();
                _autoHideTimer.Tick -= OnAutoHideTimerTick;
                _autoHideTimer = null;
            }
        }

        /// <summary>
        /// Останавливает все таймеры
        /// </summary>
        public void StopAllTimers()
        {
            StopAutoHideTimer();
            StopExpandTimer();
        }

        /// <summary>
        /// Получает длительность отображения для текущего состояния
        /// </summary>
        private int GetDisplayDuration()
        {
            return CurrentState switch
            {
                NotificationState.Compact => _config.CompactDisplayDuration,
                NotificationState.Expanded => _config.ExpandedDisplayDuration,
                NotificationState.FullyExpanded => _config.FullyExpandedDisplayDuration,
                _ => _config.CompactDisplayDuration
            };
        }

        private void OnExpandTimerTick(object sender, EventArgs e)
        {
            if (_disposed) return;

            System.Diagnostics.Debug.WriteLine("Таймер расширения сработал");
            _logger?.LogInformation("Таймер расширения сработал");
            StopExpandTimer();
            
            ChangeState(NotificationState.Expanded);
        }

        private void OnAutoHideTimerTick(object sender, EventArgs e)
        {
            if (_disposed) return;
            
            _logger?.LogDebug("Таймер автоскрытия сработал");
            StopAutoHideTimer();
            AutoHideTriggered?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopAllTimers();
                _disposed = true;
                _logger?.LogDebug("NotificationStateManager освобожден");
            }
        }
    }
}
