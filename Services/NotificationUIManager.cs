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
        public void UpdateActionsPanel(Border mainBorder, Panel actionsPanel, EventHandler<NotificationActionEventArgs> actionClicked)
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

            // Вычисляем базовую высоту контейнера для масштабирования кнопок
            var containerHeight = mainBorder?.ActualHeight > 0
                ? mainBorder.ActualHeight
                : (mainBorder?.Height > 0 ? mainBorder.Height : _config.ExpandedNotificationHeight);

            var buttonHeight = ComputeActionButtonHeight(containerHeight);

            foreach (var action in actionsToShow)
            {
                var button = CreateActionButton(action, actionClicked);
                ApplyActionButtonSizing(button, buttonHeight);
                actionsPanel.Children.Add(button);
            }

            // Высота панели действий под размер кнопки
            // Не устанавливаем фиксированную высоту для WrapPanel - пусть адаптируется к содержимому
            actionsPanel.HorizontalAlignment = HorizontalAlignment.Center;
            actionsPanel.Visibility = Visibility.Visible;
            actionsPanel.Opacity = 1.0;

            // Отладочная информация о размерах
            _logger?.LogDebug("ActionsPanel показан с {Count} кнопками. Высота кнопки: {ButtonHeight}px",
                actionsPanel.Children.Count, buttonHeight);

            // Принудительно обновляем размеры после добавления кнопок
            actionsPanel.UpdateLayout();
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
            // Адаптивная ширина: подгоняем под содержимое в разумных пределах
            var requiredWidth = ComputeRequiredWidth(mainBorder, iconContainer, contentPanel, actionButton);
            mainBorder.Width = Math.Max(_config.MinNotificationWidth, Math.Min(requiredWidth, _config.MaxNotificationWidth));
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
            var calculatedHeight = CalculateRequiredHeight(mainBorder, contentPanel);

            // Адаптивная ширина: подгоняем под содержимое
            var requiredWidth = ComputeRequiredWidth(mainBorder, iconContainer, contentPanel, actionButton);
            mainBorder.Width = Math.Max(_config.MinNotificationWidth, Math.Min(requiredWidth, _config.MaxNotificationWidth));

            mainBorder.Height = calculatedHeight;
            mainBorder.MinHeight = calculatedHeight;
            mainBorder.ClearValue(FrameworkElement.MaxHeightProperty);

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

            // После изменения размеров пересчитаем размеры уже существующих кнопок действий
            try
            {
                var actionsPanel = contentPanel?.FindName("ActionsPanel") as Panel;
                if (actionsPanel != null && actionsPanel.Children.Count > 0)
                {
                    var containerHeight = mainBorder?.ActualHeight > 0
                        ? mainBorder.ActualHeight
                        : (mainBorder?.Height > 0 ? mainBorder.Height : _config.ExpandedNotificationHeight);
                    var buttonHeight = ComputeActionButtonHeight(containerHeight);

                    foreach (var child in actionsPanel.Children)
                    {
                        if (child is Button b)
                        {
                            ApplyActionButtonSizing(b, buttonHeight);
                        }
                    }

                    actionsPanel.Visibility = Visibility.Visible;
                    actionsPanel.UpdateLayout();
                }
            }
            catch { }
        }

        private double ComputeRequiredWidth(Border mainBorder, FrameworkElement iconContainer, Panel contentPanel, FrameworkElement rightButton)
        {
            contentPanel?.UpdateLayout();
            iconContainer?.UpdateLayout();
            rightButton?.UpdateLayout();
            mainBorder?.UpdateLayout();

            var padding = mainBorder?.Padding ?? new Thickness(0);
            var iconWidth = iconContainer?.ActualWidth ?? 0;
            var contentWidth = contentPanel?.DesiredSize.Width > 0 ? contentPanel.DesiredSize.Width : contentPanel?.ActualWidth ?? 0;
            var rightWidth = rightButton?.ActualWidth ?? 0;
            var gutter = 24; // зазоры между колонками

            var required = padding.Left + iconWidth + gutter + contentWidth + gutter + rightWidth + padding.Right;
            // Минимальный запас
            required = Math.Max(required, _config.MinNotificationWidth + 80);
            return required;
        }

        private double CalculateRequiredHeight(Border mainBorder, Panel contentPanel)
        {
            // Обновляем layout, чтобы получить актуальные размеры
            contentPanel?.UpdateLayout();
            mainBorder?.UpdateLayout();

            var padding = mainBorder?.Padding ?? new Thickness(0);
            var contentHeight = contentPanel?.ActualHeight ?? 0;

            // Минимальная высота основы и запас на нижний отступ
            var baseHeight = _config.FullyExpandedBaseHeight;

            // Фактическая требуемая высота
            var required = contentHeight + padding.Top + padding.Bottom;

            // Берем максимум среди вычисленного, базового и минимального порога
            var minHeight = Math.Max(_config.MinNotificationHeight, _config.FullyExpandedMinHeight);
            var calculatedHeight = Math.Max(Math.Max(required, baseHeight), minHeight);
            return calculatedHeight;
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
            // Адаптивно от высоты: если известна текущая высота, используем коэффициенты
            var container = titleText?.Parent as FrameworkElement;
            var root = container;
            while (root != null && root is not Border)
            {
                root = root.Parent as FrameworkElement;
            }

            double baseHeight = (root as Border)?.ActualHeight > 0 ? (root as Border).ActualHeight : _config.FullyExpandedBaseHeight;

            var adaptiveTitle = Math.Max(_config.TitleFontSize, baseHeight * _config.TitleFontScale);
            var adaptiveSubtitle = Math.Max(_config.SubtitleFontSize, baseHeight * _config.SubtitleFontScale);
            var adaptiveIcon = Math.Max(_config.IconFontSize, baseHeight * _config.IconFontScale);

            titleText.FontSize = adaptiveTitle;
            subText.FontSize = adaptiveSubtitle;
            iconText.FontSize = adaptiveIcon;
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
            var contentText = new TextBlock
            {
                Text = $"{action.Icon} {action.Text}",
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap
            };

            var button = new Button
            {
                Content = contentText,
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                Tag = action,
                Height = 32,
                MinWidth = 80,
                MaxWidth = 260,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                SnapsToDevicePixels = true,
                ClipToBounds = true
            };

            button.Click += (s, e) => actionClicked?.Invoke(s, new NotificationActionEventArgs(action.Id, action.Text, action.Data));

            _logger?.LogDebug("Создана кнопка действия: {Content}", button.Content);
            return button;
        }

        private double ComputeActionButtonHeight(double containerHeight)
        {
            // Процент задается из конфигурации
            var computed = containerHeight * _config.ActionButtonHeightPercent;
            return Math.Max(_config.ActionButtonMinHeight, Math.Min(computed, _config.ActionButtonMaxHeight));
        }

        private void ApplyActionButtonSizing(Button button, double buttonHeight)
        {
            button.Height = buttonHeight;
            button.MinWidth = Math.Max(88, buttonHeight * 2.0);
            button.MaxWidth = Math.Max(200, buttonHeight * 2.6);
            button.FontSize = Math.Max(12, buttonHeight * _config.ActionButtonFontScale);
            var horizontalPadding = Math.Max(10, buttonHeight * (_config.ActionButtonFontScale * 0.9));
            var verticalPadding = Math.Max(6, buttonHeight * 0.28);
            button.Padding = new Thickness(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding);
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
