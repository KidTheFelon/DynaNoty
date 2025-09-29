using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using DynaNoty.Configuration;
using DynaNoty.Interfaces;
using Microsoft.Extensions.Logging;

namespace DynaNoty.ViewModels
{
    /// <summary>
    /// ViewModel для окна настроек
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly NotificationConfiguration _config;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;

        public INotificationService NotificationService => _notificationService;

        public event PropertyChangedEventHandler PropertyChanged;

        // Основные настройки
        private int _autoHideTimeout;
        private int _maxNotifications;
        private bool _enableAnimations;
        private bool _showActionButton;
        private bool _autoAdaptToSystemTheme;
        private bool _useSystemAccentColor;
        private bool _systemSettingsOverride;
        private int _cleanupInterval;
        private bool _showInSystemTray;
        private bool _enableSound;
        private bool _enableVibration;

        // Производительность
        private int _maxPoolSize;
        private int _preWarmCount;
        private bool _enableCaching;
        private int _maxCacheSize;

        // Размеры и позиционирование
        private double _maxWidth;
        private double _minWidth;
        private double _height;
        private double _expandedHeight;
        private double _topMargin;
        private double _verticalSpacing;
        private double _cornerRadius;
        private double _notificationAreaHeight;
        private double _iconSize;
        private double _actionButtonSize;

        // Анимации
        private int _appearDuration;
        private int _expandDuration;
        private int _repositionDuration;
        private int _expandDelay;
        private int _compactDisplayDuration;
        private int _expandedDisplayDuration;
        private int _fullyExpandedDisplayDuration;
        private bool _enableAutoExpand;

        // Стили
        private Color _backgroundColor;
        private Color _textColor;
        private Color _iconColor;
        private double _titleFontSize;
        private double _subtitleFontSize;
        private double _iconFontSize;

        // Логирование
        private bool _enableLogging;
        private int _logLevel;

        // Тема
        private int _theme;

        // Продвинутые настройки
        private int _rateLimit;
        private int _rateLimitWindowMinutes;
        private int _maxRetries;
        private int _retryDelayMs;
        private int _maxTitleLength;
        private int _maxSubtitleLength;
        private int _maxIconLength;
        private string _defaultIcon;
        private string _defaultTitle;
        private string _callIcon;
        private string _musicIcon;
        private int _performanceReportIntervalSeconds;
        private bool _enableDetailedStatistics;

        public SettingsViewModel(NotificationConfiguration config, INotificationService notificationService, ILogger logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger;

            LoadSettings();
        }

        #region Properties

        // Основные настройки
        public int AutoHideTimeout
        {
            get => _autoHideTimeout;
            set => SetProperty(ref _autoHideTimeout, value);
        }

        public int MaxNotifications
        {
            get => _maxNotifications;
            set => SetProperty(ref _maxNotifications, value);
        }

        public bool EnableAnimations
        {
            get => _enableAnimations;
            set => SetProperty(ref _enableAnimations, value);
        }

        public bool ShowActionButton
        {
            get => _showActionButton;
            set => SetProperty(ref _showActionButton, value);
        }

        public bool AutoAdaptToSystemTheme
        {
            get => _autoAdaptToSystemTheme;
            set => SetProperty(ref _autoAdaptToSystemTheme, value);
        }

        public bool UseSystemAccentColor
        {
            get => _useSystemAccentColor;
            set => SetProperty(ref _useSystemAccentColor, value);
        }

        public bool SystemSettingsOverride
        {
            get => _systemSettingsOverride;
            set => SetProperty(ref _systemSettingsOverride, value);
        }

        public int CleanupInterval
        {
            get => _cleanupInterval;
            set => SetProperty(ref _cleanupInterval, value);
        }

        public bool ShowInSystemTray
        {
            get => _showInSystemTray;
            set => SetProperty(ref _showInSystemTray, value);
        }

        public bool EnableSound
        {
            get => _enableSound;
            set => SetProperty(ref _enableSound, value);
        }

        public bool EnableVibration
        {
            get => _enableVibration;
            set => SetProperty(ref _enableVibration, value);
        }

        // Производительность
        public int MaxPoolSize
        {
            get => _maxPoolSize;
            set => SetProperty(ref _maxPoolSize, value);
        }

        public int PreWarmCount
        {
            get => _preWarmCount;
            set => SetProperty(ref _preWarmCount, value);
        }

        public bool EnableCaching
        {
            get => _enableCaching;
            set => SetProperty(ref _enableCaching, value);
        }

        public int MaxCacheSize
        {
            get => _maxCacheSize;
            set => SetProperty(ref _maxCacheSize, value);
        }

        // Размеры и позиционирование
        public double MaxWidth
        {
            get => _maxWidth;
            set => SetProperty(ref _maxWidth, value);
        }

        public double MinWidth
        {
            get => _minWidth;
            set => SetProperty(ref _minWidth, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public double ExpandedHeight
        {
            get => _expandedHeight;
            set => SetProperty(ref _expandedHeight, value);
        }

        public double TopMargin
        {
            get => _topMargin;
            set => SetProperty(ref _topMargin, value);
        }

        public double VerticalSpacing
        {
            get => _verticalSpacing;
            set => SetProperty(ref _verticalSpacing, value);
        }

        public double CornerRadius
        {
            get => _cornerRadius;
            set => SetProperty(ref _cornerRadius, value);
        }

        public double NotificationAreaHeight
        {
            get => _notificationAreaHeight;
            set => SetProperty(ref _notificationAreaHeight, value);
        }

        public double IconSize
        {
            get => _iconSize;
            set => SetProperty(ref _iconSize, value);
        }

        public double ActionButtonSize
        {
            get => _actionButtonSize;
            set => SetProperty(ref _actionButtonSize, value);
        }

        // Анимации
        public int AppearDuration
        {
            get => _appearDuration;
            set => SetProperty(ref _appearDuration, value);
        }

        public int ExpandDuration
        {
            get => _expandDuration;
            set => SetProperty(ref _expandDuration, value);
        }

        public int RepositionDuration
        {
            get => _repositionDuration;
            set => SetProperty(ref _repositionDuration, value);
        }

        public int ExpandDelay
        {
            get => _expandDelay;
            set => SetProperty(ref _expandDelay, value);
        }

        public int CompactDisplayDuration
        {
            get => _compactDisplayDuration;
            set => SetProperty(ref _compactDisplayDuration, value);
        }

        public int ExpandedDisplayDuration
        {
            get => _expandedDisplayDuration;
            set => SetProperty(ref _expandedDisplayDuration, value);
        }

        public int FullyExpandedDisplayDuration
        {
            get => _fullyExpandedDisplayDuration;
            set => SetProperty(ref _fullyExpandedDisplayDuration, value);
        }

        public bool EnableAutoExpand
        {
            get => _enableAutoExpand;
            set => SetProperty(ref _enableAutoExpand, value);
        }

        // Стили
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        public Color TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        public Color IconColor
        {
            get => _iconColor;
            set => SetProperty(ref _iconColor, value);
        }

        public double TitleFontSize
        {
            get => _titleFontSize;
            set => SetProperty(ref _titleFontSize, value);
        }

        public double SubtitleFontSize
        {
            get => _subtitleFontSize;
            set => SetProperty(ref _subtitleFontSize, value);
        }

        public double IconFontSize
        {
            get => _iconFontSize;
            set => SetProperty(ref _iconFontSize, value);
        }

        // Логирование
        public bool EnableLogging
        {
            get => _enableLogging;
            set => SetProperty(ref _enableLogging, value);
        }

        public int LogLevel
        {
            get => _logLevel;
            set => SetProperty(ref _logLevel, value);
        }

        // Тема
        public int Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        // Продвинутые настройки
        public int RateLimit
        {
            get => _rateLimit;
            set => SetProperty(ref _rateLimit, value);
        }

        public int RateLimitWindowMinutes
        {
            get => _rateLimitWindowMinutes;
            set => SetProperty(ref _rateLimitWindowMinutes, value);
        }

        public int MaxRetries
        {
            get => _maxRetries;
            set => SetProperty(ref _maxRetries, value);
        }

        public int RetryDelayMs
        {
            get => _retryDelayMs;
            set => SetProperty(ref _retryDelayMs, value);
        }

        public int MaxTitleLength
        {
            get => _maxTitleLength;
            set => SetProperty(ref _maxTitleLength, value);
        }

        public int MaxSubtitleLength
        {
            get => _maxSubtitleLength;
            set => SetProperty(ref _maxSubtitleLength, value);
        }

        public int MaxIconLength
        {
            get => _maxIconLength;
            set => SetProperty(ref _maxIconLength, value);
        }

        public string DefaultIcon
        {
            get => _defaultIcon;
            set => SetProperty(ref _defaultIcon, value);
        }

        public string DefaultTitle
        {
            get => _defaultTitle;
            set => SetProperty(ref _defaultTitle, value);
        }

        public string CallIcon
        {
            get => _callIcon;
            set => SetProperty(ref _callIcon, value);
        }

        public string MusicIcon
        {
            get => _musicIcon;
            set => SetProperty(ref _musicIcon, value);
        }

        public int PerformanceReportIntervalSeconds
        {
            get => _performanceReportIntervalSeconds;
            set => SetProperty(ref _performanceReportIntervalSeconds, value);
        }

        public bool EnableDetailedStatistics
        {
            get => _enableDetailedStatistics;
            set => SetProperty(ref _enableDetailedStatistics, value);
        }

        #endregion

        /// <summary>
        /// Загружает настройки из конфигурации
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                // Основные настройки
                AutoHideTimeout = _config.AutoHideTimeoutSeconds;
                MaxNotifications = _config.MaxNotifications;
                EnableAnimations = _config.EnableAnimations;
                ShowActionButton = _config.ShowActionButton;
                AutoAdaptToSystemTheme = _config.AutoAdaptToSystemTheme;
                UseSystemAccentColor = _config.UseSystemAccentColor;
                SystemSettingsOverride = _config.SystemSettingsOverride;
                CleanupInterval = _config.CleanupIntervalSeconds;
                ShowInSystemTray = _config.ShowInSystemTray;
                EnableSound = _config.EnableSound;
                EnableVibration = _config.EnableVibration;

                // Производительность
                MaxPoolSize = _config.MaxPoolSize;
                PreWarmCount = _config.PreWarmCount;
                EnableCaching = _config.EnableCaching;
                MaxCacheSize = _config.MaxCacheSize;

                // Размеры и позиционирование
                MaxWidth = _config.MaxNotificationWidth;
                MinWidth = _config.MinNotificationWidth;
                Height = _config.NotificationHeight;
                ExpandedHeight = _config.ExpandedNotificationHeight;
                TopMargin = _config.TopMargin;
                VerticalSpacing = _config.VerticalSpacing;
                CornerRadius = _config.CornerRadius;
                NotificationAreaHeight = _config.NotificationAreaHeight;
                IconSize = _config.IconSize;
                ActionButtonSize = _config.ActionButtonSize;

                // Анимации
                AppearDuration = _config.AppearAnimationDuration;
                ExpandDuration = _config.ExpandAnimationDuration;
                RepositionDuration = _config.RepositionAnimationDuration;
                ExpandDelay = _config.ExpandDelay;
                CompactDisplayDuration = _config.CompactDisplayDuration;
                ExpandedDisplayDuration = _config.ExpandedDisplayDuration;
                FullyExpandedDisplayDuration = _config.FullyExpandedDisplayDuration;
                EnableAutoExpand = _config.EnableAutoExpand;

                // Стили
                BackgroundColor = _config.BackgroundColor;
                TextColor = _config.TextColor;
                IconColor = _config.IconColor;
                TitleFontSize = _config.TitleFontSize;
                SubtitleFontSize = _config.SubtitleFontSize;
                IconFontSize = _config.IconFontSize;

                // Логирование
                EnableLogging = _config.EnableLogging;
                LogLevel = (int)_config.LogLevel;

                // Тема
                Theme = (int)_config.Theme;

                // Продвинутые настройки
                RateLimit = _config.Advanced.RateLimit;
                RateLimitWindowMinutes = _config.Advanced.RateLimitWindowMinutes;
                MaxRetries = _config.Advanced.MaxRetries;
                RetryDelayMs = _config.Advanced.RetryDelayMs;
                MaxTitleLength = _config.Advanced.MaxTitleLength;
                MaxSubtitleLength = _config.Advanced.MaxSubtitleLength;
                MaxIconLength = _config.Advanced.MaxIconLength;
                DefaultIcon = _config.Advanced.DefaultIcon;
                DefaultTitle = _config.Advanced.DefaultTitle;
                CallIcon = _config.Advanced.CallIcon;
                MusicIcon = _config.Advanced.MusicIcon;
                PerformanceReportIntervalSeconds = _config.Advanced.PerformanceReportIntervalSeconds;
                EnableDetailedStatistics = _config.Advanced.EnableDetailedStatistics;

                _logger?.LogInformation("Настройки загружены в ViewModel");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка загрузки настроек в ViewModel");
                throw;
            }
        }

        /// <summary>
        /// Сохраняет настройки в конфигурацию
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                // Основные настройки
                _config.AutoHideTimeoutSeconds = AutoHideTimeout;
                _config.MaxNotifications = MaxNotifications;
                _config.EnableAnimations = EnableAnimations;
                _config.ShowActionButton = ShowActionButton;
                _config.AutoAdaptToSystemTheme = AutoAdaptToSystemTheme;
                _config.UseSystemAccentColor = UseSystemAccentColor;
                _config.SystemSettingsOverride = SystemSettingsOverride;
                _config.CleanupIntervalSeconds = CleanupInterval;
                _config.ShowInSystemTray = ShowInSystemTray;
                _config.EnableSound = EnableSound;
                _config.EnableVibration = EnableVibration;

                // Производительность
                _config.MaxPoolSize = MaxPoolSize;
                _config.PreWarmCount = PreWarmCount;
                _config.EnableCaching = EnableCaching;
                _config.MaxCacheSize = MaxCacheSize;

                // Размеры и позиционирование
                _config.MaxNotificationWidth = MaxWidth;
                _config.MinNotificationWidth = MinWidth;
                _config.NotificationHeight = Height;
                _config.ExpandedNotificationHeight = ExpandedHeight;
                _config.TopMargin = TopMargin;
                _config.VerticalSpacing = VerticalSpacing;
                _config.CornerRadius = CornerRadius;
                _config.NotificationAreaHeight = NotificationAreaHeight;
                _config.IconSize = IconSize;
                _config.ActionButtonSize = ActionButtonSize;

                // Анимации
                _config.AppearAnimationDuration = AppearDuration;
                _config.ExpandAnimationDuration = ExpandDuration;
                _config.RepositionAnimationDuration = RepositionDuration;
                _config.ExpandDelay = ExpandDelay;
                _config.CompactDisplayDuration = CompactDisplayDuration;
                _config.ExpandedDisplayDuration = ExpandedDisplayDuration;
                _config.FullyExpandedDisplayDuration = FullyExpandedDisplayDuration;
                _config.EnableAutoExpand = EnableAutoExpand;

                // Стили
                _config.BackgroundColor = BackgroundColor;
                _config.TextColor = TextColor;
                _config.IconColor = IconColor;
                _config.TitleFontSize = TitleFontSize;
                _config.SubtitleFontSize = SubtitleFontSize;
                _config.IconFontSize = IconFontSize;

                // Логирование
                _config.EnableLogging = EnableLogging;
                _config.LogLevel = (Configuration.LogLevel)LogLevel;

                // Тема
                _config.Theme = (Configuration.NotificationTheme)Theme;

                // Продвинутые настройки
                _config.Advanced.RateLimit = RateLimit;
                _config.Advanced.RateLimitWindowMinutes = RateLimitWindowMinutes;
                _config.Advanced.MaxRetries = MaxRetries;
                _config.Advanced.RetryDelayMs = RetryDelayMs;
                _config.Advanced.MaxTitleLength = MaxTitleLength;
                _config.Advanced.MaxSubtitleLength = MaxSubtitleLength;
                _config.Advanced.MaxIconLength = MaxIconLength;
                _config.Advanced.DefaultIcon = DefaultIcon;
                _config.Advanced.DefaultTitle = DefaultTitle;
                _config.Advanced.CallIcon = CallIcon;
                _config.Advanced.MusicIcon = MusicIcon;
                _config.Advanced.PerformanceReportIntervalSeconds = PerformanceReportIntervalSeconds;
                _config.Advanced.EnableDetailedStatistics = EnableDetailedStatistics;

                _logger?.LogInformation("Настройки сохранены из ViewModel");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка сохранения настроек из ViewModel");
                throw;
            }
        }

        /// <summary>
        /// Сбрасывает настройки к значениям по умолчанию
        /// </summary>
        public void ResetToDefaults()
        {
            try
            {
                var defaultConfig = new NotificationConfiguration();

                // Копируем значения по умолчанию
                AutoHideTimeout = defaultConfig.AutoHideTimeoutSeconds;
                MaxNotifications = defaultConfig.MaxNotifications;
                EnableAnimations = defaultConfig.EnableAnimations;
                ShowActionButton = defaultConfig.ShowActionButton;
                MaxWidth = defaultConfig.MaxNotificationWidth;
                MinWidth = defaultConfig.MinNotificationWidth;
                Height = defaultConfig.NotificationHeight;
                ExpandedHeight = defaultConfig.ExpandedNotificationHeight;
                TopMargin = defaultConfig.TopMargin;
                VerticalSpacing = defaultConfig.VerticalSpacing;
                CornerRadius = defaultConfig.CornerRadius;
                AppearDuration = defaultConfig.AppearAnimationDuration;
                ExpandDuration = defaultConfig.ExpandAnimationDuration;
                RepositionDuration = defaultConfig.RepositionAnimationDuration;
                ExpandDelay = defaultConfig.ExpandDelay;
                CompactDisplayDuration = defaultConfig.CompactDisplayDuration;
                ExpandedDisplayDuration = defaultConfig.ExpandedDisplayDuration;
                FullyExpandedDisplayDuration = defaultConfig.FullyExpandedDisplayDuration;
                EnableAutoExpand = defaultConfig.EnableAutoExpand;
                BackgroundColor = defaultConfig.BackgroundColor;
                TextColor = defaultConfig.TextColor;
                IconColor = defaultConfig.IconColor;
                TitleFontSize = defaultConfig.TitleFontSize;
                SubtitleFontSize = defaultConfig.SubtitleFontSize;
                IconFontSize = defaultConfig.IconFontSize;
                CleanupInterval = defaultConfig.CleanupIntervalSeconds;
                ShowInSystemTray = defaultConfig.ShowInSystemTray;
                EnableSound = defaultConfig.EnableSound;
                EnableVibration = defaultConfig.EnableVibration;
                NotificationAreaHeight = defaultConfig.NotificationAreaHeight;
                IconSize = defaultConfig.IconSize;
                ActionButtonSize = defaultConfig.ActionButtonSize;
                EnableLogging = defaultConfig.EnableLogging;
                LogLevel = (int)defaultConfig.LogLevel;
                Theme = (int)defaultConfig.Theme;

                _logger?.LogInformation("Настройки сброшены к значениям по умолчанию");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка сброса настроек к значениям по умолчанию");
                throw;
            }
        }

        /// <summary>
        /// Отправляет тестовое уведомление
        /// </summary>
        public void SendTestNotification()
        {
            try
            {
                _notificationService.ShowNotification("Тест настроек", "Проверка новых параметров", "⚙️");
                _logger?.LogInformation("Тестовое уведомление отправлено");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка отправки тестового уведомления");
                throw;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
