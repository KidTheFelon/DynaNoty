using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using DynaNoty.Interfaces;
using DynaNoty.Configuration;
using DynaNoty.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DynaNoty
{
    public partial class MainWindow : Window
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<MainWindow> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(INotificationService notificationService, ILogger<MainWindow> logger = null, IServiceProvider serviceProvider = null)
        {
            InitializeComponent();
            _notificationService = notificationService ?? throw new System.ArgumentNullException(nameof(notificationService));
            _logger = logger;
            _serviceProvider = serviceProvider;

            // Подписка на события уведомлений
            _notificationService.NotificationDismissed += OnNotificationDismissed;
            _notificationService.NotificationActionClicked += OnNotificationActionClicked;

            // Добавляем поддержку перетаскивания и горячих клавиш
            this.KeyDown += MainWindow_KeyDown;
        }

        private void TestNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("Попытка показать тестовое уведомление");
                _notificationService.ShowNotification("Test Notification", "This is a test notification from DynaNoty!");
                _logger?.LogInformation("Тестовое уведомление отправлено в очередь");

                // Проверяем количество активных уведомлений
                var activeCount = _notificationService.ActiveNotificationCount;
                _logger?.LogInformation($"Активных уведомлений: {activeCount}");

                MessageBox.Show($"Уведомление отправлено! Активных: {activeCount}", "Тест", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Ошибка показа тестового уведомления");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestMusicNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _notificationService.ShowMusicNotification("Now Playing", "Bohemian Rhapsody", "Queen");
                _logger?.LogInformation("Показано музыкальное уведомление");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Ошибка показа музыкального уведомления");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestCallNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _notificationService.ShowCallNotification("Incoming Call", "John Doe", "📞");
                _logger?.LogInformation("Показано уведомление о звонке");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Ошибка показа уведомления о звонке");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void TestDetailNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var subtitle = "От: Иван Петров\nЭто детальное описание уведомления. Кликните по уведомлению, чтобы увидеть полный текст и дополнительные действия.";

                _notificationService.ShowNotification(
                    "📧 Новое сообщение",
                    subtitle,
                    "📧"
                );

                MessageBox.Show("Уведомление с детальным текстом отправлено! Кликните по нему для раскрытия.", "Тест", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка показа уведомления с детальным текстом");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestActionsNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var actions = new List<NotificationAction>
                {
                    new NotificationAction("reply", "Ответить", "reply_data", "💬"),
                    new NotificationAction("mark_read", "Прочитано", "mark_read_data", "✅"),
                    new NotificationAction("delete", "Удалить", "delete_data", "🗑️")
                };

                var subtitle = "Группа: Разработчики\nУ вас новое сообщение в чате. Вы можете ответить, отметить как прочитанное или удалить.";

                _notificationService.ShowNotification(
                    "💬 Новое сообщение в чате",
                    subtitle,
                    "💬",
                    actions
                );

                MessageBox.Show("Уведомление с действиями отправлено! Кликните по нему для раскрытия и взаимодействия.", "Тест", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка показа уведомления с действиями");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow(
                    _serviceProvider.GetRequiredService<NotificationConfiguration>(),
                    _serviceProvider.GetRequiredService<INotificationService>(),
                    _serviceProvider.GetRequiredService<ILogger<SettingsWindow>>()
                );
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка открытия окна настроек");
                MessageBox.Show($"Ошибка открытия настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _notificationService.ClearAllNotifications();
                _logger?.LogInformation("Очищены все уведомления");
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Ошибка очистки уведомлений");
            }

            Application.Current.Shutdown();
        }

        // Обработчики кастомной шапки
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Двойной клик по шапке - развернуть/свернуть окно
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            }
            else
            {
                // Перетаскивание окна
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Exit_Click(sender, e);
        }

        // Обработчик горячих клавиш
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ctrl + S - открыть настройки
            if (e.Key == System.Windows.Input.Key.S &&
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                SettingsButton_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
            // Ctrl + T - тестовое уведомление
            else if (e.Key == System.Windows.Input.Key.T &&
                     (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                TestNotification_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
            // Escape - закрыть окно
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void OnNotificationDismissed(object sender, Events.NotificationDismissedEventArgs e)
        {
            _logger?.LogDebug("Уведомление {NotificationId} закрыто в {DismissedAt}",
                e.NotificationId, e.DismissedAt);
        }

        private void OnNotificationActionClicked(object sender, Models.NotificationActionEventArgs e)
        {
            _logger?.LogDebug("Нажато действие {ActionId} - {ActionText}",
                e.ActionId, e.ActionText);
        }

        protected override void OnClosed(System.EventArgs e)
        {
            // Отписка от событий
            if (_notificationService != null)
            {
                _notificationService.NotificationDismissed -= OnNotificationDismissed;
                _notificationService.NotificationActionClicked -= OnNotificationActionClicked;
            }

            base.OnClosed(e);
        }
    }
}
