using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DynaNoty.Configuration;
using DynaNoty.Interfaces;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Провайдер конфигурации с поддержкой JSON и hot reload
    /// </summary>
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly ILogger<ConfigurationProvider> _logger;
        private readonly IConfigurationValidator _validator;
        private FileSystemWatcher _fileWatcher;
        private readonly string _configPath;
        private NotificationConfiguration _currentConfig;
        private bool _disposed = false;

        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        public ConfigurationProvider(
            string configPath = "appsettings.json",
            IConfigurationValidator validator = null,
            ILogger<ConfigurationProvider> logger = null)
        {
            _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
            _validator = validator ?? new ConfigurationValidator();
            _logger = logger;

            // Загружаем конфигурацию при инициализации
            LoadConfiguration();

            // Настраиваем мониторинг файла для hot reload
            SetupFileWatcher();
        }

        /// <summary>
        /// Получает текущую конфигурацию
        /// </summary>
        public NotificationConfiguration GetConfiguration()
        {
            return _currentConfig ?? new NotificationConfiguration();
        }

        /// <summary>
        /// Обновляет конфигурацию
        /// </summary>
        public async Task<bool> UpdateConfigurationAsync(NotificationConfiguration newConfig)
        {
            if (newConfig == null)
                throw new ArgumentNullException(nameof(newConfig));

            try
            {
                // Валидируем новую конфигурацию
                var validationResult = _validator.ValidateAndFix(newConfig);
                if (!validationResult.IsValid)
                {
                    _logger?.LogError("Конфигурация не прошла валидацию: {Errors}", 
                        string.Join(", ", validationResult.Errors));
                    return false;
                }

                var oldConfig = _currentConfig;
                _currentConfig = newConfig;

                // Сохраняем в файл
                await SaveConfigurationAsync();

                // Уведомляем об изменении
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(oldConfig, newConfig));

                _logger?.LogInformation("Конфигурация обновлена");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обновления конфигурации");
                return false;
            }
        }

        /// <summary>
        /// Сохраняет конфигурацию в файл
        /// </summary>
        public async Task SaveConfigurationAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_currentConfig, options);
                await File.WriteAllTextAsync(_configPath, json);

                _logger?.LogDebug("Конфигурация сохранена в {ConfigPath}", _configPath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка сохранения конфигурации в {ConfigPath}", _configPath);
                throw;
            }
        }

        /// <summary>
        /// Загружает конфигурацию из файла
        /// </summary>
        public async Task LoadConfigurationAsync()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    _logger?.LogInformation("Файл конфигурации {ConfigPath} не найден, используем настройки по умолчанию", _configPath);
                    _currentConfig = new NotificationConfiguration();
                    await SaveConfigurationAsync(); // Создаем файл с настройками по умолчанию
                    return;
                }

                var json = await File.ReadAllTextAsync(_configPath);
                var config = JsonSerializer.Deserialize<NotificationConfiguration>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (config == null)
                {
                    _logger?.LogWarning("Не удалось десериализовать конфигурацию из {ConfigPath}", _configPath);
                    _currentConfig = new NotificationConfiguration();
                    return;
                }

                // Валидируем загруженную конфигурацию
                _validator.ValidateAndFix(config);
                _currentConfig = config;

                _logger?.LogInformation("Конфигурация загружена из {ConfigPath}", _configPath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка загрузки конфигурации из {ConfigPath}", _configPath);
                _currentConfig = new NotificationConfiguration();
            }
        }

        private void LoadConfiguration()
        {
            _ = LoadConfigurationAsync();
        }

        private void SetupFileWatcher()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configPath);
                var fileName = Path.GetFileName(_configPath);

                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                    return;

                _fileWatcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true
                };

                _fileWatcher.Changed += OnConfigFileChanged;
                _fileWatcher.Created += OnConfigFileChanged;

                _logger?.LogDebug("Мониторинг файла конфигурации настроен для {ConfigPath}", _configPath);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Не удалось настроить мониторинг файла конфигурации");
            }
        }

        private async void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Небольшая задержка, чтобы файл успел освободиться
                await Task.Delay(100);
                await LoadConfigurationAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обработки изменения файла конфигурации");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _fileWatcher?.Dispose();
                _disposed = true;
                _logger?.LogDebug("ConfigurationProvider освобожден");
            }
        }
    }

}
