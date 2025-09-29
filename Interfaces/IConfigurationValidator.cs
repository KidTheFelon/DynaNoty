using System.Collections.Generic;
using DynaNoty.Configuration;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для валидации конфигурации
    /// </summary>
    public interface IConfigurationValidator
    {
        /// <summary>
        /// Валидирует конфигурацию и исправляет некорректные значения
        /// </summary>
        ValidationResult ValidateAndFix(NotificationConfiguration config);

        /// <summary>
        /// Проверяет, является ли конфигурация валидной
        /// </summary>
        bool IsValid(NotificationConfiguration config);
    }

    /// <summary>
    /// Результат валидации конфигурации
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
