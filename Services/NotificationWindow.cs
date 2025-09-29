using System;
using System.Windows;
using System.Windows.Controls;
using DynaNoty.Interfaces;
using DynaNoty.Configuration;

namespace DynaNoty.Services
{
    /// <summary>
    /// Реализация окна для отображения уведомлений
    /// </summary>
    public class NotificationWindow : INotificationWindow
    {
        private readonly Window _window;
        private readonly Canvas _container;
        private readonly NotificationConfiguration _config;
        private bool _disposed = false;

        public NotificationWindow(NotificationConfiguration config = null)
        {
            _config = config ?? new NotificationConfiguration();
            
            _container = new Canvas
            {
                Width = SystemParameters.PrimaryScreenWidth,
                Height = _config.NotificationAreaHeight,
                IsHitTestVisible = true // Изменили на true для видимости
            };

            _window = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                Topmost = true,
                Left = 0,
                Top = 0,
                Width = SystemParameters.PrimaryScreenWidth,
                Height = _config.NotificationAreaHeight,
                IsHitTestVisible = true,
                ShowInTaskbar = false
            };

            _window.Content = _container;
        }

        public Panel Container => _container;

        public void Show()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NotificationWindow));
                
            System.Diagnostics.Debug.WriteLine($"NotificationWindow.Show() вызван. Окно видимо: {_window.Visibility}");
            
            _window.Show();
            
            System.Diagnostics.Debug.WriteLine($"NotificationWindow показан. Видимость: {_window.Visibility}, Детей в контейнере: {_container.Children.Count}");
        }

        public void Hide()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NotificationWindow));
                
            _window.Hide();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _window?.Close();
                _disposed = true;
            }
        }
    }
}
