using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using DynaNoty.Interfaces;
using DynaNoty.Configuration;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для валидации и санитизации входных данных
    /// </summary>
    public class InputValidationService : IInputValidationService
    {
        private readonly ILogger<InputValidationService> _logger;
        private readonly int _maxStringLength;
        private readonly int _maxIconLength;

        public InputValidationService(NotificationConfiguration config, ILogger<InputValidationService> logger = null)
        {
            _logger = logger;
            _maxStringLength = config?.Advanced?.MaxTitleLength ?? 100;
            _maxIconLength = config?.Advanced?.MaxIconLength ?? 10;
        }

        // Приватный конструктор для внутреннего использования
        private InputValidationService(int maxStringLength, int maxIconLength, ILogger<InputValidationService> logger)
        {
            _logger = logger;
            _maxStringLength = maxStringLength;
            _maxIconLength = maxIconLength;
        }

        /// <summary>
        /// Валидирует и санитизирует строку для уведомления
        /// </summary>
        public string ValidateAndSanitizeString(string input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                var error = $"{fieldName} не может быть пустым";
                _logger?.LogWarning("Валидация не пройдена: {Error}", error);
                throw new ArgumentException(error, fieldName);
            }

            var sanitized = SanitizeString(input);

            if (sanitized.Length > _maxStringLength)
            {
                sanitized = sanitized.Substring(0, _maxStringLength);
                _logger?.LogWarning("Строка {FieldName} обрезана до {MaxLength} символов", fieldName, _maxStringLength);
            }

            _logger?.LogDebug("Строка {FieldName} успешно валидирована и санитизирована", fieldName);
            return sanitized;
        }

        /// <summary>
        /// Валидирует и санитизирует иконку
        /// </summary>
        public string ValidateAndSanitizeIcon(string icon, string fieldName = "Icon")
        {
            if (string.IsNullOrWhiteSpace(icon))
            {
                var error = $"{fieldName} не может быть пустым";
                _logger?.LogWarning("Валидация иконки не пройдена: {Error}", error);
                throw new ArgumentException(error, fieldName);
            }

            var sanitized = SanitizeString(icon);

            if (sanitized.Length > _maxIconLength)
            {
                sanitized = sanitized.Substring(0, _maxIconLength);
                _logger?.LogWarning("Иконка {FieldName} обрезана до {MaxLength} символов", fieldName, _maxIconLength);
            }

            // Проверяем, что это действительно эмодзи или символ
            if (!IsValidIcon(sanitized))
            {
                _logger?.LogWarning("Иконка {FieldName} содержит недопустимые символы: {Icon}", fieldName, sanitized);
            }

            _logger?.LogDebug("Иконка {FieldName} успешно валидирована и санитизирована", fieldName);
            return sanitized;
        }

        /// <summary>
        /// Валидирует данные для обычного уведомления
        /// </summary>
        public (string title, string subtitle, string icon) ValidateStandardNotification(string title, string subtitle, string icon)
        {
            var validatedTitle = ValidateAndSanitizeString(title, nameof(title));
            var validatedSubtitle = ValidateAndSanitizeString(subtitle, nameof(subtitle));
            var validatedIcon = ValidateAndSanitizeIcon(icon, nameof(icon));

            _logger?.LogDebug("Данные обычного уведомления успешно валидированы");
            return (validatedTitle, validatedSubtitle, validatedIcon);
        }

        /// <summary>
        /// Валидирует данные для музыкального уведомления
        /// </summary>
        public (string title, string subtitle, string artist) ValidateMusicNotification(string title, string subtitle, string artist)
        {
            var validatedTitle = ValidateAndSanitizeString(title, nameof(title));
            var validatedSubtitle = ValidateAndSanitizeString(subtitle, nameof(subtitle));
            var validatedArtist = ValidateAndSanitizeString(artist, nameof(artist));

            _logger?.LogDebug("Данные музыкального уведомления успешно валидированы");
            return (validatedTitle, validatedSubtitle, validatedArtist);
        }

        /// <summary>
        /// Валидирует данные для уведомления о звонке
        /// </summary>
        public (string title, string caller, string icon) ValidateCallNotification(string title, string caller, string icon)
        {
            var validatedTitle = ValidateAndSanitizeString(title, nameof(title));
            var validatedCaller = ValidateAndSanitizeString(caller, nameof(caller));
            var validatedIcon = ValidateAndSanitizeIcon(icon, nameof(icon));

            _logger?.LogDebug("Данные уведомления о звонке успешно валидированы");
            return (validatedTitle, validatedCaller, validatedIcon);
        }

        /// <summary>
        /// Валидирует данные для компактного уведомления
        /// </summary>
        public string ValidateCompactNotification(string icon)
        {
            var validatedIcon = ValidateAndSanitizeIcon(icon, nameof(icon));

            _logger?.LogDebug("Данные компактного уведомления успешно валидированы");
            return validatedIcon;
        }

        /// <summary>
        /// Санитизирует строку, удаляя потенциально опасные символы
        /// </summary>
        private string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Удаляем HTML теги
            var sanitized = Regex.Replace(input, @"<[^>]+>", "");

            // Удаляем скрипты
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);

            // Удаляем управляющие символы и потенциально опасные
            sanitized = sanitized.Trim()
                .Replace("\r", "")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace("\0", ""); // Null символы

            // Удаляем множественные пробелы
            sanitized = Regex.Replace(sanitized, @"\s+", " ");

            return sanitized;
        }

        /// <summary>
        /// Проверяет, является ли строка валидной иконкой
        /// </summary>
        private bool IsValidIcon(string icon)
        {
            if (string.IsNullOrEmpty(icon))
                return false;

            // Проверяем, что это эмодзи или простой символ
            // Эмодзи обычно занимают 2-4 байта в UTF-8
            var bytes = System.Text.Encoding.UTF8.GetBytes(icon);
            return bytes.Length <= 8 && !ContainsControlCharacters(icon);
        }

        /// <summary>
        /// Проверяет, содержит ли строка управляющие символы
        /// </summary>
        private bool ContainsControlCharacters(string input)
        {
            foreach (char c in input)
            {
                if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                {
                    return true;
                }
            }
            return false;
        }
    }
}
