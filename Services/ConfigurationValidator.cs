using System;
using System.Collections.Generic;
using System.Linq;
using DynaNoty.Configuration;
using DynaNoty.Constants;
using DynaNoty.Interfaces;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Валидатор конфигурации уведомлений
    /// </summary>
    public class ConfigurationValidator : IConfigurationValidator
    {
        private readonly ILogger<ConfigurationValidator> _logger;

        public ConfigurationValidator(ILogger<ConfigurationValidator> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Валидирует конфигурацию и исправляет некорректные значения
        /// </summary>
        public Interfaces.ValidationResult ValidateAndFix(NotificationConfiguration config)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Валидация размеров
            if (config.MaxNotificationWidth <= 0)
            {
                config.MaxNotificationWidth = NotificationConstants.DEFAULT_MAX_WIDTH;
                warnings.Add($"MaxNotificationWidth установлена в {NotificationConstants.DEFAULT_MAX_WIDTH}");
            }

            if (config.MinNotificationWidth <= 0)
            {
                config.MinNotificationWidth = NotificationConstants.DEFAULT_MIN_WIDTH;
                warnings.Add($"MinNotificationWidth установлена в {NotificationConstants.DEFAULT_MIN_WIDTH}");
            }

            if (config.MinNotificationWidth >= config.MaxNotificationWidth)
            {
                config.MinNotificationWidth = config.MaxNotificationWidth * 0.2;
                warnings.Add($"MinNotificationWidth скорректирована до {config.MinNotificationWidth}");
            }

            if (config.NotificationHeight <= 0)
            {
                config.NotificationHeight = NotificationConstants.DEFAULT_HEIGHT;
                warnings.Add($"NotificationHeight установлена в {NotificationConstants.DEFAULT_HEIGHT}");
            }

            // Валидация таймаутов
            if (config.AutoHideTimeoutSeconds <= 0)
            {
                config.AutoHideTimeoutSeconds = NotificationConstants.DEFAULT_AUTO_HIDE_TIMEOUT;
                warnings.Add($"AutoHideTimeoutSeconds установлен в {NotificationConstants.DEFAULT_AUTO_HIDE_TIMEOUT}");
            }

            if (config.CleanupIntervalSeconds <= 0)
            {
                config.CleanupIntervalSeconds = NotificationConstants.DEFAULT_CLEANUP_INTERVAL;
                warnings.Add($"CleanupIntervalSeconds установлен в {NotificationConstants.DEFAULT_CLEANUP_INTERVAL}");
            }

            // Валидация лимитов
            if (config.MaxNotifications <= 0)
            {
                config.MaxNotifications = NotificationConstants.DEFAULT_MAX_NOTIFICATIONS;
                warnings.Add($"MaxNotifications установлен в {NotificationConstants.DEFAULT_MAX_NOTIFICATIONS}");
            }

            if (config.MaxPoolSize <= 0)
            {
                config.MaxPoolSize = NotificationConstants.DEFAULT_MAX_POOL_SIZE;
                warnings.Add($"MaxPoolSize установлен в {NotificationConstants.DEFAULT_MAX_POOL_SIZE}");
            }

            // Валидация анимаций
            if (config.AppearAnimationDuration < 0)
            {
                config.AppearAnimationDuration = NotificationConstants.DEFAULT_APPEAR_DURATION;
                warnings.Add($"AppearAnimationDuration установлена в {NotificationConstants.DEFAULT_APPEAR_DURATION}");
            }

            if (config.ExpandAnimationDuration < 0)
            {
                config.ExpandAnimationDuration = NotificationConstants.DEFAULT_EXPAND_DURATION;
                warnings.Add($"ExpandAnimationDuration установлена в {NotificationConstants.DEFAULT_EXPAND_DURATION}");
            }

            // Валидация шрифтов
            if (config.TitleFontSize <= 0)
            {
                config.TitleFontSize = 16;
                warnings.Add($"TitleFontSize установлен в 16");
            }

            if (config.SubtitleFontSize <= 0)
            {
                config.SubtitleFontSize = 14;
                warnings.Add($"SubtitleFontSize установлен в 14");
            }

            if (config.IconFontSize <= 0)
            {
                config.IconFontSize = 20;
                warnings.Add($"IconFontSize установлен в 20");
            }

            // Валидация размеров элементов
            if (config.IconSize <= 0)
            {
                config.IconSize = NotificationConstants.DEFAULT_ICON_SIZE;
                warnings.Add($"IconSize установлен в {NotificationConstants.DEFAULT_ICON_SIZE}");
            }

            if (config.ActionButtonSize <= 0)
            {
                config.ActionButtonSize = NotificationConstants.DEFAULT_BUTTON_SIZE;
                warnings.Add($"ActionButtonSize установлен в {NotificationConstants.DEFAULT_BUTTON_SIZE}");
            }

            // Логирование результатов
            if (warnings.Any())
            {
                _logger?.LogWarning("Конфигурация исправлена: {Warnings}", string.Join(", ", warnings));
            }

            if (errors.Any())
            {
                _logger?.LogError("Ошибки в конфигурации: {Errors}", string.Join(", ", errors));
            }

            return new Interfaces.ValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }

        /// <summary>
        /// Проверяет, является ли конфигурация валидной
        /// </summary>
        public bool IsValid(NotificationConfiguration config)
        {
            return ValidateAndFix(config).IsValid;
        }
    }

}
