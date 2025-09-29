using System;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для ограничения частоты запросов
    /// </summary>
    public interface IRateLimiter : IDisposable
    {
        /// <summary>
        /// Проверяет, можно ли выполнить запрос
        /// </summary>
        bool CanMakeRequest(string key = "default");

        /// <summary>
        /// Получает время до следующего разрешенного запроса
        /// </summary>
        TimeSpan? GetTimeUntilNextRequest(string key = "default");

        /// <summary>
        /// Очищает историю для ключа
        /// </summary>
        void ClearHistory(string key = "default");

        /// <summary>
        /// Очищает всю историю
        /// </summary>
        void ClearAllHistory();
    }
}
