using System;
using DynaNoty;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using DynaNoty.Configuration;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Builder для создания уведомлений с удобным API
    /// </summary>
    public class NotificationBuilder
    {
        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private string _icon = "🔔";
        private NotificationType _type = NotificationType.Standard;

        /// <summary>
        /// Создает новый экземпляр NotificationBuilder
        /// </summary>
        public static NotificationBuilder Create()
        {
            return new NotificationBuilder();
        }

        /// <summary>
        /// Создает стандартное уведомление
        /// </summary>
        public static NotificationBuilder CreateStandard()
        {
            return new NotificationBuilder()
                .WithType(NotificationType.Standard);
        }

        /// <summary>
        /// Создает музыкальное уведомление
        /// </summary>
        public static NotificationBuilder CreateMusic()
        {
            return new NotificationBuilder()
                .WithType(NotificationType.Music)
                .WithIcon("🎵");
        }

        /// <summary>
        /// Создает уведомление о звонке
        /// </summary>
        public static NotificationBuilder CreateCall()
        {
            return new NotificationBuilder()
                .WithType(NotificationType.Call)
                .WithIcon("📞");
        }

        /// <summary>
        /// Создает компактное уведомление
        /// </summary>
        public static NotificationBuilder CreateCompact()
        {
            return new NotificationBuilder()
                .WithType(NotificationType.Compact)
                .WithTitle(string.Empty)
                .WithSubtitle(string.Empty);
        }

        /// <summary>
        /// Устанавливает заголовок
        /// </summary>
        public NotificationBuilder WithTitle(string title)
        {
            _title = title ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Устанавливает подзаголовок
        /// </summary>
        public NotificationBuilder WithSubtitle(string subtitle)
        {
            _subtitle = subtitle ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Устанавливает иконку
        /// </summary>
        public NotificationBuilder WithIcon(string icon)
        {
            _icon = icon ?? "🔔";
            return this;
        }

        /// <summary>
        /// Устанавливает тип уведомления
        /// </summary>
        public NotificationBuilder WithType(NotificationType type)
        {
            _type = type;
            return this;
        }

        /// <summary>
        /// Создает NotificationData
        /// </summary>
        public NotificationData Build()
        {
            return new NotificationData(_title, _subtitle, _icon, _type);
        }

        /// <summary>
        /// Создает музыкальное уведомление с дополнительными параметрами
        /// </summary>
        public NotificationData BuildMusic(string song, string artist)
        {
            return NotificationData.CreateMusicNotification(_title, song, artist);
        }

        /// <summary>
        /// Создает уведомление о звонке с дополнительными параметрами
        /// </summary>
        public NotificationData BuildCall(string caller)
        {
            return NotificationData.CreateCallNotification(_title, caller, _icon);
        }

        /// <summary>
        /// Создает компактное уведомление
        /// </summary>
        public NotificationData BuildCompact()
        {
            return NotificationData.CreateCompactNotification(_icon);
        }
    }

    /// <summary>
    /// Builder для создания конфигурации уведомлений
    /// </summary>
    public class NotificationConfigurationBuilder
    {
        private readonly NotificationConfiguration _config;

        /// <summary>
        /// Создает новый экземпляр NotificationConfigurationBuilder
        /// </summary>
        public NotificationConfigurationBuilder()
        {
            _config = new NotificationConfiguration();
        }

        /// <summary>
        /// Создает новый экземпляр с базовой конфигурацией
        /// </summary>
        public static NotificationConfigurationBuilder Create()
        {
            return new NotificationConfigurationBuilder();
        }

        /// <summary>
        /// Создает конфигурацию для быстрых уведомлений
        /// </summary>
        public static NotificationConfigurationBuilder CreateFast()
        {
            return new NotificationConfigurationBuilder()
                .WithAutoHideTimeout(2)
                .WithExpandDelay(200)
                .WithAnimations(false);
        }

        /// <summary>
        /// Создает конфигурацию для красивых анимированных уведомлений
        /// </summary>
        public static NotificationConfigurationBuilder CreateAnimated()
        {
            return new NotificationConfigurationBuilder()
                .WithAutoHideTimeout(5)
                .WithExpandDelay(500)
                .WithAnimations(true)
                .WithAppearDuration(400)
                .WithExpandDuration(600);
        }

        /// <summary>
        /// Устанавливает время автоскрытия
        /// </summary>
        public NotificationConfigurationBuilder WithAutoHideTimeout(int seconds)
        {
            _config.AutoHideTimeoutSeconds = seconds;
            return this;
        }

        /// <summary>
        /// Устанавливает задержку перед расширением
        /// </summary>
        public NotificationConfigurationBuilder WithExpandDelay(int milliseconds)
        {
            _config.ExpandDelay = milliseconds;
            return this;
        }

        /// <summary>
        /// Включает/выключает анимации
        /// </summary>
        public NotificationConfigurationBuilder WithAnimations(bool enabled)
        {
            _config.EnableAnimations = enabled;
            return this;
        }

        /// <summary>
        /// Устанавливает длительность анимации появления
        /// </summary>
        public NotificationConfigurationBuilder WithAppearDuration(int milliseconds)
        {
            _config.AppearAnimationDuration = milliseconds;
            return this;
        }

        /// <summary>
        /// Устанавливает длительность анимации расширения
        /// </summary>
        public NotificationConfigurationBuilder WithExpandDuration(int milliseconds)
        {
            _config.ExpandAnimationDuration = milliseconds;
            return this;
        }

        /// <summary>
        /// Устанавливает максимальное количество уведомлений
        /// </summary>
        public NotificationConfigurationBuilder WithMaxNotifications(int count)
        {
            _config.MaxNotifications = count;
            return this;
        }

        /// <summary>
        /// Устанавливает размеры уведомления
        /// </summary>
        public NotificationConfigurationBuilder WithSizes(double minWidth, double maxWidth, double height)
        {
            _config.MinNotificationWidth = minWidth;
            _config.MaxNotificationWidth = maxWidth;
            _config.NotificationHeight = height;
            return this;
        }

        /// <summary>
        /// Устанавливает цвета
        /// </summary>
        public NotificationConfigurationBuilder WithColors(
            System.Windows.Media.Color backgroundColor,
            System.Windows.Media.Color textColor,
            System.Windows.Media.Color iconColor)
        {
            _config.BackgroundColor = backgroundColor;
            _config.TextColor = textColor;
            _config.IconColor = iconColor;
            return this;
        }

        /// <summary>
        /// Устанавливает системную тему
        /// </summary>
        public NotificationConfigurationBuilder WithSystemTheme(bool autoAdapt = true, bool useAccentColor = true)
        {
            _config.AutoAdaptToSystemTheme = autoAdapt;
            _config.UseSystemAccentColor = useAccentColor;
            return this;
        }

        /// <summary>
        /// Устанавливает настройки производительности
        /// </summary>
        public NotificationConfigurationBuilder WithPerformance(int maxPoolSize = 10, int preWarmCount = 3)
        {
            _config.MaxPoolSize = maxPoolSize;
            _config.PreWarmCount = preWarmCount;
            return this;
        }

        /// <summary>
        /// Устанавливает размеры шрифтов
        /// </summary>
        public NotificationConfigurationBuilder WithFontSizes(double titleSize = 16, double subtitleSize = 14, double iconSize = 20)
        {
            _config.TitleFontSize = titleSize;
            _config.SubtitleFontSize = subtitleSize;
            _config.IconFontSize = iconSize;
            return this;
        }

        /// <summary>
        /// Устанавливает размеры элементов UI
        /// </summary>
        public NotificationConfigurationBuilder WithElementSizes(double iconSize = 24, double buttonSize = 24)
        {
            _config.IconSize = iconSize;
            _config.ActionButtonSize = buttonSize;
            return this;
        }

        /// <summary>
        /// Создает NotificationConfiguration
        /// </summary>
        public NotificationConfiguration Build()
        {
            return _config;
        }
    }

    /// <summary>
    /// Builder для создания NotificationManager
    /// </summary>
    public class NotificationManagerBuilder
    {
        private NotificationConfiguration _config;
        private ILoggerFactory _loggerFactory;
        private INotificationWindow _window;
        private bool _useDefaultServices = true;

        /// <summary>
        /// Создает новый экземпляр NotificationManagerBuilder
        /// </summary>
        public static NotificationManagerBuilder Create()
        {
            return new NotificationManagerBuilder();
        }

        /// <summary>
        /// Создает builder с быстрой конфигурацией
        /// </summary>
        public static NotificationManagerBuilder CreateFast()
        {
            return new NotificationManagerBuilder()
                .WithConfiguration(NotificationConfigurationBuilder.CreateFast().Build());
        }

        /// <summary>
        /// Создает builder с анимированной конфигурацией
        /// </summary>
        public static NotificationManagerBuilder CreateAnimated()
        {
            return new NotificationManagerBuilder()
                .WithConfiguration(NotificationConfigurationBuilder.CreateAnimated().Build());
        }

        /// <summary>
        /// Устанавливает конфигурацию
        /// </summary>
        public NotificationManagerBuilder WithConfiguration(NotificationConfiguration config)
        {
            _config = config;
            return this;
        }

        /// <summary>
        /// Устанавливает логгер
        /// </summary>
        public NotificationManagerBuilder WithLogger(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            return this;
        }

        /// <summary>
        /// Устанавливает окно уведомлений
        /// </summary>
        public NotificationManagerBuilder WithWindow(INotificationWindow window)
        {
            _window = window;
            return this;
        }

        /// <summary>
        /// Отключает использование сервисов по умолчанию
        /// </summary>
        public NotificationManagerBuilder WithoutDefaultServices()
        {
            _useDefaultServices = false;
            return this;
        }

        /// <summary>
        /// Создает NotificationManager
        /// </summary>
        public NotificationManager Build()
        {
            if (_useDefaultServices)
            {
                return NotificationManagerFactory.CreateWithConfig(_config ?? new NotificationConfiguration(), _loggerFactory);
            }

            var dependencies = new NotificationManagerDependencies
            {
                NotificationWindow = _window,
                Config = _config ?? new NotificationConfiguration(),
                LoggerFactory = _loggerFactory
            };

            return new NotificationManager(dependencies);
        }
    }
}
