namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для валидации и санитизации входных данных
    /// </summary>
    public interface IInputValidationService
    {
        /// <summary>
        /// Валидирует и санитизирует строку для уведомления
        /// </summary>
        string ValidateAndSanitizeString(string input, string fieldName);

        /// <summary>
        /// Валидирует и санитизирует иконку
        /// </summary>
        string ValidateAndSanitizeIcon(string icon, string fieldName = "Icon");

        /// <summary>
        /// Валидирует данные для обычного уведомления
        /// </summary>
        (string title, string subtitle, string icon) ValidateStandardNotification(string title, string subtitle, string icon);

        /// <summary>
        /// Валидирует данные для музыкального уведомления
        /// </summary>
        (string title, string subtitle, string artist) ValidateMusicNotification(string title, string subtitle, string artist);

        /// <summary>
        /// Валидирует данные для уведомления о звонке
        /// </summary>
        (string title, string caller, string icon) ValidateCallNotification(string title, string caller, string icon);

        /// <summary>
        /// Валидирует данные для компактного уведомления
        /// </summary>
        string ValidateCompactNotification(string icon);
    }
}
