using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DynaNoty.Configuration;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Менеджер UI уведомления
    /// </summary>
    public class NotificationUIManager
    {
        private readonly NotificationConfiguration _config;
        private readonly ISystemThemeService _themeService;
        private readonly ILogger _logger;
        private List<NotificationAction> _actions = new List<NotificationAction>();

        public NotificationUIManager(
            NotificationConfiguration config,
            ISystemThemeService themeService,
            ILogger logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _logger = logger;
        }

        /// <summary>
        /// Настраивает содержимое уведомления
        /// </summary>
        public void SetupContent(
            TextBlock titleText,
            TextBlock subText,
            TextBlock iconText,
            string title,
            string subtitle,
            string icon)
        {
            titleText.Text = title;
            subText.Text = subtitle;
            iconText.Text = icon;

            _logger?.LogDebug("Содержимое уведомления настроено: {Title} - {Subtitle}", title, subtitle);
        }

        /// <summary>
        /// Настраивает действия уведомления
        /// </summary>
        public void SetupActions(List<NotificationAction> actions)
        {
            _actions = actions ?? new List<NotificationAction>();
            _logger?.LogDebug("Действия уведомления настроены. Количество: {Count}", _actions.Count);
        }

        /// <summary>
        /// Применяет цвета к элементам UI
        /// </summary>
        public void ApplyColors(
            Border mainBorder,
            TextBlock titleText,
            TextBlock subText,
            TextBlock iconText,
            FrameworkElement iconContainer,
            Button actionButton)
        {
            var colors = GetThemeColors();

            mainBorder.Background = new SolidColorBrush(colors.BackgroundColor);
            titleText.Foreground = new SolidColorBrush(colors.TextColor);
            subText.Foreground = new SolidColorBrush(colors.TextColor);
            iconText.Foreground = new SolidColorBrush(colors.IconColor);

            ApplyFontSizes(titleText, subText, iconText);
            ApplySizes(iconContainer, actionButton);

            _logger?.LogDebug("Цвета применены к UI элементам");
        }

        /// <summary>
        /// Устанавливает режим отображения текста
        /// </summary>
        public void SetTextDisplayMode(TextBlock titleText, TextBlock subText, bool isExpanded, bool isFullyExpanded = false)
        {
            if (isFullyExpanded)
            {
                // Полностью раскрытое состояние - полный текст
                titleText.TextTrimming = TextTrimming.None;
                titleText.TextWrapping = TextWrapping.Wrap;
                subText.TextTrimming = TextTrimming.None;
                subText.TextWrapping = TextWrapping.Wrap;
            }
            else if (isExpanded)
            {
                // Первое раскрытие - только одна строка подзаголовка с троеточиями
                titleText.TextTrimming = TextTrimming.CharacterEllipsis;
                titleText.TextWrapping = TextWrapping.NoWrap;
                subText.TextTrimming = TextTrimming.CharacterEllipsis;
                subText.TextWrapping = TextWrapping.NoWrap;
            }
            else
            {
                // Компактное состояние - троеточия
                titleText.TextTrimming = TextTrimming.CharacterEllipsis;
                titleText.TextWrapping = TextWrapping.NoWrap;
                subText.TextTrimming = TextTrimming.CharacterEllipsis;
                subText.TextWrapping = TextWrapping.NoWrap;
            }
        }

        /// <summary>
        /// Обновляет панель действий
        /// </summary>
        public void UpdateActionsPanel(StackPanel actionsPanel, EventHandler<NotificationActionEventArgs> actionClicked)
        {
            actionsPanel.Children.Clear();

            if (_actions == null || _actions.Count == 0)
            {
                actionsPanel.Visibility = Visibility.Collapsed;
                _logger?.LogDebug("ActionsPanel скрыт - нет действий");
                return;
            }

            var actionsToShow = _actions.Take(2).ToList();
            _logger?.LogDebug("Показываем максимум 2 действия из {TotalCount}", _actions.Count);

            foreach (var action in actionsToShow)
            {
                var button = CreateActionButton(action, actionClicked);
                actionsPanel.Children.Add(button);
            }

            actionsPanel.Visibility = Visibility.Visible;
            actionsPanel.Opacity = 1.0;
            _logger?.LogDebug("ActionsPanel показан с {Count} кнопками", actionsPanel.Children.Count);
        }

        /// <summary>
        /// Устанавливает компактный размер уведомления
        /// </summary>
        public void SetCompactSize(Border mainBorder, UserControl userControl, Panel contentPanel, Button actionButton, FrameworkElement iconContainer = null)
        {
            mainBorder.Width = _config.MinNotificationWidth;
            mainBorder.MinHeight = _config.MinNotificationHeight;
            mainBorder.ClearValue(FrameworkElement.HeightProperty);
            mainBorder.ClearValue(FrameworkElement.MaxHeightProperty);

            userControl.MinHeight = _config.MinNotificationHeight;
            userControl.ClearValue(FrameworkElement.HeightProperty);
            userControl.ClearValue(FrameworkElement.MaxHeightProperty);

            contentPanel.Visibility = Visibility.Collapsed;
            actionButton.Visibility = Visibility.Collapsed;

            // В компактном состоянии иконка по центру
            if (iconContainer != null)
            {
                Grid.SetColumn(iconContainer, 1); // Центральная колонка
            }
        }

        /// <summary>
        /// Устанавливает расширенный размер уведомления
        /// </summary>
        public void SetExpandedSize(Border mainBorder, Panel contentPanel, Button actionButton, FrameworkElement iconContainer = null)
        {
            mainBorder.Width = _config.MaxNotificationWidth;
            mainBorder.Height = _config.ExpandedNotificationHeight;
            mainBorder.MinHeight = _config.ExpandedNotificationHeight;

            contentPanel.Visibility = Visibility.Visible;
            contentPanel.Opacity = 1.0;

            if (actionButton != null)
            {
                actionButton.Visibility = Visibility.Visible;
                actionButton.Opacity = 1.0;
            }

            // В расширенном состоянии иконка слева
            if (iconContainer != null)
            {
                Grid.SetColumn(iconContainer, 0); // Левая колонка
            }
        }

        /// <summary>
        /// Устанавливает полностью раскрытый размер уведомления
        /// </summary>
        public void SetFullyExpandedSize(Border mainBorder, Panel contentPanel, Button actionButton, FrameworkElement iconContainer = null)
        {
            var baseHeight = _config.FullyExpandedBaseHeight;
            var actionsHeight = (_actions != null && _actions.Count > 0) ? _config.ActionsPanelHeight : 0.0;
            var calculatedHeight = baseHeight + actionsHeight;

            mainBorder.Height = calculatedHeight;
            mainBorder.MinHeight = calculatedHeight;
            mainBorder.ClearValue(FrameworkElement.MaxHeightProperty);
            mainBorder.Width = _config.MaxNotificationWidth;

            contentPanel.Visibility = Visibility.Visible;
            contentPanel.Opacity = 1.0;

            if (actionButton != null)
            {
                actionButton.Visibility = Visibility.Visible;
                actionButton.Opacity = 1.0;
            }

            // В полностью раскрытом состоянии иконка слева
            if (iconContainer != null)
            {
                Grid.SetColumn(iconContainer, 0); // Левая колонка
            }

            _logger?.LogDebug("Установлен полностью раскрытый размер: {Height}px", calculatedHeight);
        }

        /// <summary>
        /// Получает цвета темы
        /// </summary>
        private (Color BackgroundColor, Color TextColor, Color IconColor) GetThemeColors()
        {
            var backgroundColor = _config.BackgroundColor;
            var textColor = _config.TextColor;
            var iconColor = _config.IconColor;

            if (_config.AutoAdaptToSystemTheme)
            {
                if (_config.SystemSettingsOverride || backgroundColor == Colors.Black)
                {
                    backgroundColor = _themeService.GetRecommendedBackgroundColor();
                }

                if (_config.SystemSettingsOverride || textColor == Colors.White)
                {
                    textColor = _themeService.GetRecommendedTextColor();
                }
            }

            if (_config.UseSystemAccentColor)
            {
                if (_config.SystemSettingsOverride || iconColor == Colors.White)
                {
                    iconColor = _themeService.GetRecommendedIconColor();
                }
            }

            return (backgroundColor, textColor, iconColor);
        }

        /// <summary>
        /// Применяет размеры шрифтов
        /// </summary>
        private void ApplyFontSizes(TextBlock titleText, TextBlock subText, TextBlock iconText)
        {
            titleText.FontSize = _config.TitleFontSize;
            subText.FontSize = _config.SubtitleFontSize;
            iconText.FontSize = _config.IconFontSize;
        }

        /// <summary>
        /// Применяет размеры элементов
        /// </summary>
        private void ApplySizes(FrameworkElement iconContainer, Button actionButton)
        {
            iconContainer.Width = _config.IconSize;
            iconContainer.Height = _config.IconSize;
            actionButton.Width = _config.ActionButtonSize;
            actionButton.Height = _config.ActionButtonSize;
        }

        /// <summary>
        /// Создает кнопку действия
        /// </summary>
        private Button CreateActionButton(NotificationAction action, EventHandler<NotificationActionEventArgs> actionClicked)
        {
            var button = new Button
            {
                Content = $"{action.Icon} {action.Text}",
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                Tag = action,
                Height = 32,
                MinWidth = 80
            };

            button.Click += (s, e) => actionClicked?.Invoke(s, new NotificationActionEventArgs(action.Id, action.Text, action.Data));

            _logger?.LogDebug("Создана кнопка действия: {Content}", button.Content);
            return button;
        }

        /// <summary>
        /// Освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            // Очищаем список действий
            _actions?.Clear();
            _actions = null;

            _logger?.LogDebug("NotificationUIManager освобожден");
        }
    }
}
