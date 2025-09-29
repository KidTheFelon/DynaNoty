using System;
using System.Threading.Tasks;
using DynaNoty.Configuration;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для провайдера конфигурации
    /// </summary>
    public interface IConfigurationProvider : IDisposable
    {
        /// <summary>
        /// Событие изменения конфигурации
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        /// <summary>
        /// Получает текущую конфигурацию
        /// </summary>
        NotificationConfiguration GetConfiguration();

        /// <summary>
        /// Обновляет конфигурацию
        /// </summary>
        Task<bool> UpdateConfigurationAsync(NotificationConfiguration newConfig);

        /// <summary>
        /// Сохраняет конфигурацию в файл
        /// </summary>
        Task SaveConfigurationAsync();

        /// <summary>
        /// Загружает конфигурацию из файла
        /// </summary>
        Task LoadConfigurationAsync();
    }

    /// <summary>
    /// Аргументы события изменения конфигурации
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public NotificationConfiguration OldConfiguration { get; }
        public NotificationConfiguration NewConfiguration { get; }

        public ConfigurationChangedEventArgs(NotificationConfiguration oldConfig, NotificationConfiguration newConfig)
        {
            OldConfiguration = oldConfig;
            NewConfiguration = newConfig;
        }
    }
}
