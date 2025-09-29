using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для работы с системной темой и акцентными цветами Windows
    /// </summary>
    public class SystemThemeService : ISystemThemeService
    {
        private readonly ILogger<SystemThemeService> _logger;
        private bool _isDarkTheme = false;
        private Color _accentColor = Colors.Blue;
        private System.Windows.Threading.DispatcherTimer _themeCheckTimer;

        public SystemThemeService(ILogger<SystemThemeService> logger = null)
        {
            _logger = logger;
            DetectSystemTheme();
            DetectAccentColor();
            SetupThemeMonitoring();
        }

        /// <summary>
        /// Определяет, включена ли темная тема в системе
        /// </summary>
        public bool IsDarkTheme => _isDarkTheme;

        /// <summary>
        /// Получает акцентный цвет системы
        /// </summary>
        public Color AccentColor => _accentColor;

        /// <summary>
        /// Событие изменения системной темы
        /// </summary>
        public event System.EventHandler SystemThemeChanged;

        /// <summary>
        /// Получает рекомендуемый цвет фона для уведомлений
        /// </summary>
        public Color GetRecommendedBackgroundColor()
        {
            if (_isDarkTheme)
            {
                return Color.FromRgb(28, 28, 30); // Темный фон
            }
            else
            {
                return Color.FromRgb(255, 255, 255); // Светлый фон
            }
        }

        /// <summary>
        /// Получает рекомендуемый цвет текста для уведомлений
        /// </summary>
        public Color GetRecommendedTextColor()
        {
            if (_isDarkTheme)
            {
                return Color.FromRgb(255, 255, 255); // Белый текст
            }
            else
            {
                return Color.FromRgb(0, 0, 0); // Черный текст
            }
        }

        /// <summary>
        /// Получает рекомендуемый цвет иконки для уведомлений
        /// </summary>
        public Color GetRecommendedIconColor()
        {
            return _accentColor; // Используем акцентный цвет
        }

        /// <summary>
        /// Определяет системную тему
        /// </summary>
        private void DetectSystemTheme()
        {
            try
            {
                // Проверяем реестр Windows для определения темы
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key?.GetValue("AppsUseLightTheme") is int lightTheme)
                    {
                        _isDarkTheme = lightTheme == 0;
                        _logger?.LogDebug("Системная тема определена: {Theme}", _isDarkTheme ? "Темная" : "Светлая");
                    }
                    else
                    {
                        // Fallback: определяем по системным цветам
                        _isDarkTheme = SystemParameters.WindowGlassBrush is SolidColorBrush brush && 
                                     brush.Color.R + brush.Color.G + brush.Color.B < 384; // 384 = 128*3
                        _logger?.LogDebug("Системная тема определена через fallback: {Theme}", _isDarkTheme ? "Темная" : "Светлая");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка определения системной темы");
                _isDarkTheme = false; // По умолчанию светлая тема
            }
        }

        /// <summary>
        /// Определяет акцентный цвет системы
        /// </summary>
        private void DetectAccentColor()
        {
            try
            {
                // Пытаемся получить акцентный цвет из реестра
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
                {
                    if (key?.GetValue("AccentColor") is int accentColorValue)
                    {
                        // Акцентный цвет хранится в формате ARGB
                        var colorBytes = BitConverter.GetBytes(accentColorValue);
                        _accentColor = Color.FromArgb(
                            colorBytes[3], // Alpha
                            colorBytes[0], // Red
                            colorBytes[1], // Green
                            colorBytes[2]  // Blue
                        );
                        _logger?.LogDebug("Акцентный цвет определен: {Color}", _accentColor);
                    }
                    else
                    {
                        // Fallback: используем системный акцентный цвет
                        _accentColor = (SystemParameters.WindowGlassBrush as SolidColorBrush)?.Color ?? Colors.Blue;
                        _logger?.LogDebug("Акцентный цвет определен через fallback: {Color}", _accentColor);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка определения акцентного цвета");
                _accentColor = Colors.Blue; // По умолчанию синий
            }
        }

        /// <summary>
        /// Обновляет определение темы и акцентного цвета
        /// </summary>
        public void Refresh()
        {
            var oldTheme = _isDarkTheme;
            var oldAccent = _accentColor;
            
            DetectSystemTheme();
            DetectAccentColor();
            
            // Проверяем, изменилась ли тема или акцентный цвет
            if (oldTheme != _isDarkTheme || oldAccent != _accentColor)
            {
                SystemThemeChanged?.Invoke(this, System.EventArgs.Empty);
                _logger?.LogInformation("Системная тема или акцентный цвет изменились");
            }
            
            // Логируем только при изменениях
            if (oldTheme != _isDarkTheme || oldAccent != _accentColor)
            {
                _logger?.LogInformation("Системные настройки обновлены");
            }
        }

        /// <summary>
        /// Настраивает мониторинг изменений системной темы
        /// </summary>
        private void SetupThemeMonitoring()
        {
            try
            {
                _themeCheckTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(30) // Проверяем каждые 30 секунд
                };
                _themeCheckTimer.Tick += OnThemeCheckTimerTick;
                _themeCheckTimer.Start();
                
                _logger?.LogDebug("Мониторинг системной темы настроен");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Не удалось настроить мониторинг системной темы");
            }
        }

        /// <summary>
        /// Обработчик таймера проверки темы
        /// </summary>
        private void OnThemeCheckTimerTick(object sender, System.EventArgs e)
        {
            try
            {
                Refresh();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при проверке системной темы");
            }
        }

        /// <summary>
        /// Освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            _themeCheckTimer?.Stop();
            _themeCheckTimer = null;
            _logger?.LogDebug("SystemThemeService освобожден");
        }
    }
}
