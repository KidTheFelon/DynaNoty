using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Валидатор настроек
    /// </summary>
    public class SettingsValidator
    {
        private readonly ILogger _logger;

        public SettingsValidator(ILogger logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Валидирует числовое поле
        /// </summary>
        public bool ValidateNumericField(TextBox field, string fieldName)
        {
            if (string.IsNullOrEmpty(field.Text))
            {
                SetFieldError(field, "Поле не может быть пустым");
                return false;
            }

            if (double.TryParse(field.Text, out double value))
            {
                bool isValid = IsValidNumericValue(fieldName, value);
                SetFieldError(field, isValid ? null : GetValidationMessage(fieldName, value));
                return isValid;
            }

            SetFieldError(field, "Некорректное числовое значение");
            return false;
        }

        /// <summary>
        /// Проверяет валидность числового значения для конкретного поля
        /// </summary>
        private bool IsValidNumericValue(string fieldName, double value)
        {
            return fieldName switch
            {
                "AutoHideTimeoutTextBox" => value >= 1 && value <= 60,
                "MaxNotificationsTextBox" => value >= 1 && value <= 10,
                "MaxWidthTextBox" => value >= 100 && value <= 800,
                "MinWidthTextBox" => value >= 30 && value <= 200,
                "HeightTextBox" => value >= 30 && value <= 200,
                "ExpandedHeightTextBox" => value >= 60 && value <= 300,
                "TopMarginTextBox" => value >= 0 && value <= 200,
                "VerticalSpacingTextBox" => value >= 20 && value <= 200,
                "MaxPoolSizeTextBox" => value >= 1 && value <= 50,
                "PreWarmCountTextBox" => value >= 0 && value <= 20,
                "MaxCacheSizeTextBox" => value >= 10 && value <= 1000,
                "CornerRadiusTextBox" => value >= 0 && value <= 50,
                "AppearDurationTextBox" => value >= 100 && value <= 2000,
                "ExpandDurationTextBox" => value >= 100 && value <= 2000,
                "RepositionDurationTextBox" => value >= 100 && value <= 2000,
                "ExpandDelayTextBox" => value >= 0 && value <= 2000,
                "CompactDisplayDurationTextBox" => value >= 1000 && value <= 10000,
                "ExpandedDisplayDurationTextBox" => value >= 1000 && value <= 15000,
                "FullyExpandedDisplayDurationTextBox" => value >= 1000 && value <= 20000,
                "TitleFontSizeTextBox" => value >= 8 && value <= 32,
                "SubtitleFontSizeTextBox" => value >= 8 && value <= 24,
                "IconFontSizeTextBox" => value >= 8 && value <= 48,
                "CleanupIntervalTextBox" => value >= 1 && value <= 60,
                "NotificationAreaHeightTextBox" => value >= 100 && value <= 1000,
                "IconSizeTextBox" => value >= 8 && value <= 64,
                "ActionButtonSizeTextBox" => value >= 8 && value <= 64,
                _ => true
            };
        }

        /// <summary>
        /// Получает сообщение валидации для поля
        /// </summary>
        private string GetValidationMessage(string fieldName, double value)
        {
            return fieldName switch
            {
                "AutoHideTimeoutTextBox" => "Таймаут должен быть от 1 до 60 секунд",
                "MaxNotificationsTextBox" => "Максимальное количество уведомлений должно быть от 1 до 10",
                "MaxWidthTextBox" => "Максимальная ширина должна быть от 100 до 800 пикселей",
                "MinWidthTextBox" => "Минимальная ширина должна быть от 30 до 200 пикселей",
                "HeightTextBox" => "Высота должна быть от 30 до 200 пикселей",
                "ExpandedHeightTextBox" => "Высота расширенного уведомления должна быть от 60 до 300 пикселей",
                "TopMarginTextBox" => "Верхний отступ должен быть от 0 до 200 пикселей",
                "VerticalSpacingTextBox" => "Вертикальный интервал должен быть от 20 до 200 пикселей",
                "MaxPoolSizeTextBox" => "Размер пула должен быть от 1 до 50",
                "PreWarmCountTextBox" => "Количество предварительно созданных уведомлений должно быть от 0 до 20",
                "MaxCacheSizeTextBox" => "Размер кэша должен быть от 10 до 1000",
                "CornerRadiusTextBox" => "Радиус углов должен быть от 0 до 50 пикселей",
                "AppearDurationTextBox" => "Длительность анимации появления должна быть от 100 до 2000 мс",
                "ExpandDurationTextBox" => "Длительность анимации расширения должна быть от 100 до 2000 мс",
                "RepositionDurationTextBox" => "Длительность анимации перепозиционирования должна быть от 100 до 2000 мс",
                "ExpandDelayTextBox" => "Задержка расширения должна быть от 0 до 2000 мс",
                "CompactDisplayDurationTextBox" => "Длительность отображения компактного уведомления должна быть от 1000 до 10000 мс",
                "ExpandedDisplayDurationTextBox" => "Длительность отображения расширенного уведомления должна быть от 1000 до 15000 мс",
                "FullyExpandedDisplayDurationTextBox" => "Длительность отображения полностью раскрытого уведомления должна быть от 1000 до 20000 мс",
                "TitleFontSizeTextBox" => "Размер шрифта заголовка должен быть от 8 до 32 пикселей",
                "SubtitleFontSizeTextBox" => "Размер шрифта подзаголовка должен быть от 8 до 24 пикселей",
                "IconFontSizeTextBox" => "Размер шрифта иконки должен быть от 8 до 48 пикселей",
                "CleanupIntervalTextBox" => "Интервал очистки должен быть от 1 до 60 секунд",
                "NotificationAreaHeightTextBox" => "Высота области уведомлений должна быть от 100 до 1000 пикселей",
                "IconSizeTextBox" => "Размер иконки должен быть от 8 до 64 пикселей",
                "ActionButtonSizeTextBox" => "Размер кнопки действия должен быть от 8 до 64 пикселей",
                _ => "Некорректное значение"
            };
        }

        /// <summary>
        /// Устанавливает ошибку для поля
        /// </summary>
        private void SetFieldError(TextBox field, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                field.BorderBrush = Brushes.LimeGreen;
                field.ToolTip = null;
            }
            else
            {
                field.BorderBrush = Brushes.Red;
                field.ToolTip = errorMessage;
            }
        }

        /// <summary>
        /// Валидирует все числовые поля
        /// </summary>
        public bool ValidateAllNumericFields(IEnumerable<TextBox> fields)
        {
            bool allValid = true;
            foreach (var field in fields)
            {
                if (!ValidateNumericField(field, field.Name))
                {
                    allValid = false;
                }
            }
            return allValid;
        }

        /// <summary>
        /// Валидирует цветовое поле
        /// </summary>
        public bool ValidateColorField(Color? color, string fieldName)
        {
            if (!color.HasValue)
            {
                _logger?.LogWarning("Цветовое поле {FieldName} не выбрано", fieldName);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Валидирует все настройки
        /// </summary>
        public ValidationResult ValidateAllSettings(Dictionary<string, object> settings)
        {
            var errors = new List<string>();

            try
            {
                // Валидация основных настроек
                if (!IsInRange((int)settings["AutoHideTimeout"], 1, 60))
                    errors.Add("Таймаут автоскрытия должен быть от 1 до 60 секунд");

                if (!IsInRange((int)settings["MaxNotifications"], 1, 10))
                    errors.Add("Максимальное количество уведомлений должно быть от 1 до 10");

                // Валидация размеров
                if (!IsInRange((double)settings["MaxWidth"], 100, 800))
                    errors.Add("Максимальная ширина должна быть от 100 до 800 пикселей");

                if (!IsInRange((double)settings["MinWidth"], 30, 200))
                    errors.Add("Минимальная ширина должна быть от 30 до 200 пикселей");

                // Валидация анимаций
                if (!IsInRange((int)settings["AppearDuration"], 100, 2000))
                    errors.Add("Длительность анимации появления должна быть от 100 до 2000 мс");

                if (!IsInRange((int)settings["ExpandDuration"], 100, 2000))
                    errors.Add("Длительность анимации расширения должна быть от 100 до 2000 мс");

                _logger?.LogDebug("Валидация настроек завершена. Найдено ошибок: {ErrorCount}", errors.Count);

                return new ValidationResult
                {
                    IsValid = errors.Count == 0,
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка валидации настроек");
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { "Ошибка валидации настроек" }
                };
            }
        }

        private bool IsInRange(double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        private bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }
    }

    /// <summary>
    /// Результат валидации
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
