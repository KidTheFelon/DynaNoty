using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DynaNoty.Configuration;
using DynaNoty.Interfaces;
using DynaNoty.ViewModels;
using DynaNoty.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DynaNoty
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;
        private readonly SettingsValidator _validator;
        private readonly SettingsMapper _mapper;
        private readonly ILogger<SettingsWindow> _logger;
        private Dictionary<string, Control> _uiElements;

        public SettingsWindow(NotificationConfiguration config, INotificationService notificationService, ILogger<SettingsWindow> logger)
        {
            InitializeComponent();
            _logger = logger;

            _viewModel = new SettingsViewModel(config, notificationService, logger);
            _validator = new SettingsValidator(logger);
            _mapper = new SettingsMapper(logger);

            InitializeUI();
            LoadSettings();

            // Добавляем поддержку навигации по клавишам
            this.KeyDown += SettingsWindow_KeyDown;
        }

        /// <summary>
        /// Инициализирует UI элементы
        /// </summary>
        private void InitializeUI()
        {
            _uiElements = new Dictionary<string, Control>
            {
                // Основные настройки
                { "AutoHideTimeoutTextBox", AutoHideTimeoutTextBox },
                { "MaxNotificationsTextBox", MaxNotificationsTextBox },
                { "EnableAnimationsCheckBox", EnableAnimationsCheckBox },
                { "ShowActionButtonCheckBox", ShowActionButtonCheckBox },
                { "AutoAdaptToSystemThemeCheckBox", AutoAdaptToSystemThemeCheckBox },
                { "UseSystemAccentColorCheckBox", UseSystemAccentColorCheckBox },
                { "SystemSettingsOverrideCheckBox", SystemSettingsOverrideCheckBox },
                { "CleanupIntervalTextBox", CleanupIntervalTextBox },
                { "ShowInSystemTrayCheckBox", ShowInSystemTrayCheckBox },
                { "EnableSoundCheckBox", EnableSoundCheckBox },
                { "EnableVibrationCheckBox", EnableVibrationCheckBox },

                // Производительность
                { "MaxPoolSizeTextBox", MaxPoolSizeTextBox },
                { "PreWarmCountTextBox", PreWarmCountTextBox },
                { "EnableCachingCheckBox", EnableCachingCheckBox },
                { "MaxCacheSizeTextBox", MaxCacheSizeTextBox },

                // Размеры и позиционирование
                { "MaxWidthTextBox", MaxWidthTextBox },
                { "MinWidthTextBox", MinWidthTextBox },
                { "HeightTextBox", HeightTextBox },
                { "ExpandedHeightTextBox", ExpandedHeightTextBox },
                { "TopMarginTextBox", TopMarginTextBox },
                { "VerticalSpacingTextBox", VerticalSpacingTextBox },
                { "CornerRadiusTextBox", CornerRadiusTextBox },
                { "NotificationAreaHeightTextBox", NotificationAreaHeightTextBox },
                { "IconSizeTextBox", IconSizeTextBox },
                { "ActionButtonSizeTextBox", ActionButtonSizeTextBox },

                // Анимации
                { "AppearDurationTextBox", AppearDurationTextBox },
                { "ExpandDurationTextBox", ExpandDurationTextBox },
                { "RepositionDurationTextBox", RepositionDurationTextBox },
                { "ExpandDelayTextBox", ExpandDelayTextBox },
                { "CompactDisplayDurationTextBox", CompactDisplayDurationTextBox },
                { "ExpandedDisplayDurationTextBox", ExpandedDisplayDurationTextBox },
                { "FullyExpandedDisplayDurationTextBox", FullyExpandedDisplayDurationTextBox },
                { "EnableAutoExpandCheckBox", EnableAutoExpandCheckBox },

                // Стили
                { "BackgroundColorPicker", BackgroundColorPicker },
                { "TextColorPicker", TextColorPicker },
                { "IconColorPicker", IconColorPicker },
                { "TitleFontSizeTextBox", TitleFontSizeTextBox },
                { "SubtitleFontSizeTextBox", SubtitleFontSizeTextBox },
                { "IconFontSizeTextBox", IconFontSizeTextBox },

                // Логирование
                { "EnableLoggingCheckBox", EnableLoggingCheckBox },
                { "LogLevelComboBox", LogLevelComboBox },

                // Тема
                { "ThemeComboBox", ThemeComboBox }
            };

            AddValidationHandlers();
            AddEventHandlers();
        }

        private void LoadSettings()
        {
            try
            {
                _viewModel.LoadSettings();
                _mapper.MapViewModelToUI(_viewModel, _uiElements);
                UpdateColorControlsVisibility((Configuration.NotificationTheme)_viewModel.Theme);
                _logger?.LogInformation("Настройки загружены в окно конфигурации");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка загрузки настроек");
                ShowErrorMessage($"Ошибка загрузки настроек: {ex.Message}");
            }
        }

        private void AddValidationHandlers()
        {
            // Валидация числовых полей
            var numericFields = _uiElements.Values.OfType<TextBox>().ToArray();

            foreach (var field in numericFields)
            {
                field.TextChanged += (s, e) => ValidateNumericField(field);
                field.LostFocus += (s, e) => ValidateNumericField(field);
            }
        }

        /// <summary>
        /// Добавляет обработчики событий
        /// </summary>
        private void AddEventHandlers()
        {
            // Обработчик изменения темы
            ThemeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;

            // Обработчики изменения цветов
            BackgroundColorPicker.SelectedColorChanged += ColorPicker_SelectionChanged;
            TextColorPicker.SelectedColorChanged += ColorPicker_SelectionChanged;
            IconColorPicker.SelectedColorChanged += ColorPicker_SelectionChanged;
        }

        private void ValidateNumericField(TextBox field)
        {
            _validator.ValidateNumericField(field, field.Name);
        }


        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "⚠️ Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "✅ Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveSettings()
        {
            try
            {
                _mapper.MapUIToViewModel(_uiElements, _viewModel);
                _viewModel.SaveSettings();
                _logger?.LogInformation("Настройки сохранены");
                ShowSuccessMessage("Настройки сохранены успешно!");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка сохранения настроек");
                ShowErrorMessage($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Применяем настройки временно для теста
                SaveSettings();

                // Показываем тестовое уведомление
                _viewModel.SendTestNotification();
                ShowSuccessMessage("Тестовое уведомление отправлено!");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка отправки тестового уведомления");
                ShowErrorMessage($"Ошибка отправки тестового уведомления: {ex.Message}");
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Сбросить все настройки к значениям по умолчанию?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.ResetToDefaults();
                    _mapper.MapViewModelToUI(_viewModel, _uiElements);
                    UpdateColorControlsVisibility((Configuration.NotificationTheme)_viewModel.Theme);

                    _logger?.LogInformation("Настройки сброшены к значениям по умолчанию");
                    ShowSuccessMessage("Настройки сброшены к значениям по умолчанию!");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка сброса настроек");
                ShowErrorMessage($"Ошибка сброса настроек: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(MainTabButton, MainTabContent);
        }

        private void SizeTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(SizeTabButton, SizeTabContent);
        }

        private void AnimationTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(AnimationTabButton, AnimationTabContent);
        }

        private void StyleTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(StyleTabButton, StyleTabContent);
        }


        private void PerformanceTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(PerformanceTabButton, PerformanceTabContent);
        }

        private void AdvancedTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(AdvancedTabButton, AdvancedTabContent);
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ThemeComboBox.SelectedIndex >= 0)
                {
                    var selectedTheme = (Configuration.NotificationTheme)ThemeComboBox.SelectedIndex;
                    ApplyTheme(selectedTheme);
                    UpdateColorControlsVisibility(selectedTheme);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка изменения темы");
            }
        }

        private void ApplyThemeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Применяем выбранную тему
                var selectedTheme = (Configuration.NotificationTheme)ThemeComboBox.SelectedIndex;
                ApplyTheme(selectedTheme);

                _logger?.LogInformation($"Применена тема: {selectedTheme}");
                ShowSuccessMessage($"Тема '{GetThemeDisplayName(selectedTheme)}' применена!");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка применения темы");
                ShowErrorMessage($"Ошибка применения темы: {ex.Message}");
            }
        }

        private void RefreshStatsButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshPerformanceStats();
        }

        /// <summary>
        /// Применяет выбранную тему к конфигурации
        /// </summary>
        private void ApplyTheme(Configuration.NotificationTheme theme)
        {
            switch (theme)
            {
                case Configuration.NotificationTheme.System:
                    // Системная тема - используем текущие настройки
                    break;

                case Configuration.NotificationTheme.Dark:
                    _viewModel.BackgroundColor = System.Windows.Media.Colors.Black;
                    _viewModel.TextColor = System.Windows.Media.Colors.White;
                    _viewModel.IconColor = System.Windows.Media.Colors.White;
                    break;

                case Configuration.NotificationTheme.Light:
                    _viewModel.BackgroundColor = System.Windows.Media.Colors.White;
                    _viewModel.TextColor = System.Windows.Media.Colors.Black;
                    _viewModel.IconColor = System.Windows.Media.Colors.Black;
                    break;

                case Configuration.NotificationTheme.Blue:
                    _viewModel.BackgroundColor = System.Windows.Media.Color.FromRgb(0, 120, 255);
                    _viewModel.TextColor = System.Windows.Media.Colors.White;
                    _viewModel.IconColor = System.Windows.Media.Colors.White;
                    break;

                case Configuration.NotificationTheme.Green:
                    _viewModel.BackgroundColor = System.Windows.Media.Color.FromRgb(52, 199, 89);
                    _viewModel.TextColor = System.Windows.Media.Colors.White;
                    _viewModel.IconColor = System.Windows.Media.Colors.White;
                    break;

                case Configuration.NotificationTheme.Purple:
                    _viewModel.BackgroundColor = System.Windows.Media.Color.FromRgb(175, 82, 222);
                    _viewModel.TextColor = System.Windows.Media.Colors.White;
                    _viewModel.IconColor = System.Windows.Media.Colors.White;
                    break;

                case Configuration.NotificationTheme.Orange:
                    _viewModel.BackgroundColor = System.Windows.Media.Color.FromRgb(255, 149, 0);
                    _viewModel.TextColor = System.Windows.Media.Colors.White;
                    _viewModel.IconColor = System.Windows.Media.Colors.White;
                    break;

                case Configuration.NotificationTheme.Pink:
                    _viewModel.BackgroundColor = System.Windows.Media.Color.FromRgb(255, 45, 85);
                    _viewModel.TextColor = System.Windows.Media.Colors.White;
                    _viewModel.IconColor = System.Windows.Media.Colors.White;
                    break;

                case Configuration.NotificationTheme.Custom:
                    // Пользовательская тема - используем текущие настройки из конфигурации
                    // Не изменяем цвета, они уже установлены пользователем
                    // Отключаем автоматическую подстройку под системную тему
                    _viewModel.AutoAdaptToSystemTheme = false;
                    break;
            }

            // Обновляем UI с новыми цветами (только для готовых тем)
            if (theme != Configuration.NotificationTheme.Custom)
            {
                // Включаем автоматическую подстройку для готовых тем
                _viewModel.AutoAdaptToSystemTheme = true;

                BackgroundColorPicker.SelectedColor = _viewModel.BackgroundColor;
                TextColorPicker.SelectedColor = _viewModel.TextColor;
                IconColorPicker.SelectedColor = _viewModel.IconColor;
            }

            // Обновляем предварительный просмотр
            UpdateThemePreview();
        }

        /// <summary>
        /// Обработчик изменения цвета в ColorPicker
        /// </summary>
        private void ColorPicker_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            try
            {
                // Обновляем ViewModel с новыми цветами
                if (BackgroundColorPicker.SelectedColor.HasValue)
                    _viewModel.BackgroundColor = BackgroundColorPicker.SelectedColor.Value;
                if (TextColorPicker.SelectedColor.HasValue)
                    _viewModel.TextColor = TextColorPicker.SelectedColor.Value;
                if (IconColorPicker.SelectedColor.HasValue)
                    _viewModel.IconColor = IconColorPicker.SelectedColor.Value;

                // Отключаем автоматическую подстройку при ручном изменении цветов
                _viewModel.AutoAdaptToSystemTheme = false;

                // Обновляем предварительный просмотр
                UpdateThemePreview();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обновления цветов");
            }
        }

        /// <summary>
        /// Обновляет видимость элементов выбора цветов в зависимости от темы
        /// </summary>
        private void UpdateColorControlsVisibility(Configuration.NotificationTheme theme)
        {
            try
            {
                bool showColorControls = (theme == Configuration.NotificationTheme.Custom);

                // Управляем видимостью секции цветовой схемы
                if (ColorSchemeSection != null)
                {
                    ColorSchemeSection.Visibility = showColorControls ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обновления видимости элементов выбора цветов");
            }
        }

        /// <summary>
        /// Обновляет предварительный просмотр темы
        /// </summary>
        private void UpdateThemePreview()
        {
            try
            {
                if (PreviewNotification != null)
                {
                    PreviewNotification.Background = new System.Windows.Media.SolidColorBrush(_viewModel.BackgroundColor);
                    PreviewTitle.Foreground = new System.Windows.Media.SolidColorBrush(_viewModel.TextColor);
                    PreviewSubtitle.Foreground = new System.Windows.Media.SolidColorBrush(_viewModel.TextColor);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обновления предварительного просмотра темы");
            }
        }

        /// <summary>
        /// Возвращает отображаемое имя темы
        /// </summary>
        private string GetThemeDisplayName(Configuration.NotificationTheme theme)
        {
            return theme switch
            {
                Configuration.NotificationTheme.System => "Системная",
                Configuration.NotificationTheme.Dark => "Темная",
                Configuration.NotificationTheme.Light => "Светлая",
                Configuration.NotificationTheme.Blue => "Синяя",
                Configuration.NotificationTheme.Green => "Зеленая",
                Configuration.NotificationTheme.Purple => "Фиолетовая",
                Configuration.NotificationTheme.Orange => "Оранжевая",
                Configuration.NotificationTheme.Pink => "Розовая",
                Configuration.NotificationTheme.Custom => "Пользовательская",
                _ => "Неизвестная"
            };
        }

        /// <summary>
        /// Обновляет статистику производительности
        /// </summary>
        private void RefreshPerformanceStats()
        {
            try
            {
                if (_viewModel.NotificationService is NotificationManager manager)
                {
                    var stats = manager.GetPerformanceStats();
                    ActiveNotificationsText.Text = stats.Active.ToString();
                    PoolSizeText.Text = stats.PoolSize.ToString();
                    ReuseRateText.Text = $"{stats.ReuseRate}%";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка обновления статистики производительности");
            }
        }

        private void SetActiveTab(Button activeButton, StackPanel activeContent)
        {
            // Сбрасываем все кнопки
            MainTabButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2C2C2E"));
            SizeTabButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2C2C2E"));
            AnimationTabButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2C2C2E"));
            StyleTabButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2C2C2E"));
            PerformanceTabButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2C2C2E"));
            AdvancedTabButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2C2C2E"));

            // Скрываем все ScrollViewer'ы
            MainScrollViewer.Visibility = Visibility.Collapsed;
            SizeScrollViewer.Visibility = Visibility.Collapsed;
            AnimationScrollViewer.Visibility = Visibility.Collapsed;
            StyleScrollViewer.Visibility = Visibility.Collapsed;
            PerformanceScrollViewer.Visibility = Visibility.Collapsed;
            AdvancedScrollViewer.Visibility = Visibility.Collapsed;

            // Активируем выбранную кнопку и ScrollViewer
            activeButton.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF007AFF"));

            // Прокручиваем к активной вкладке, если она не видна
            ScrollToActiveTab(activeButton);

            // Определяем какой ScrollViewer показать
            if (activeContent == MainTabContent)
                MainScrollViewer.Visibility = Visibility.Visible;
            else if (activeContent == SizeTabContent)
                SizeScrollViewer.Visibility = Visibility.Visible;
            else if (activeContent == AnimationTabContent)
                AnimationScrollViewer.Visibility = Visibility.Visible;
            else if (activeContent == StyleTabContent)
                StyleScrollViewer.Visibility = Visibility.Visible;
            else if (activeContent == PerformanceTabContent)
                PerformanceScrollViewer.Visibility = Visibility.Visible;
            else if (activeContent == AdvancedTabContent)
                AdvancedScrollViewer.Visibility = Visibility.Visible;

            // Сбрасываем прокрутку в начало
            if (activeContent == MainTabContent)
                MainScrollViewer.ScrollToTop();
            else if (activeContent == SizeTabContent)
                SizeScrollViewer.ScrollToTop();
            else if (activeContent == AnimationTabContent)
                AnimationScrollViewer.ScrollToTop();
            else if (activeContent == StyleTabContent)
                StyleScrollViewer.ScrollToTop();
            else if (activeContent == PerformanceTabContent)
                PerformanceScrollViewer.ScrollToTop();
            else if (activeContent == AdvancedTabContent)
                AdvancedScrollViewer.ScrollToTop();
        }

        /// <summary>
        /// Прокручивает к активной вкладке, если она не видна
        /// </summary>
        private void ScrollToActiveTab(Button activeButton)
        {
            try
            {
                if (TabsScrollViewer == null || activeButton == null) return;

                // Получаем позицию кнопки относительно ScrollViewer
                var buttonPosition = activeButton.TranslatePoint(new System.Windows.Point(0, 0), TabsScrollViewer);
                var scrollViewerWidth = TabsScrollViewer.ActualWidth;
                var buttonWidth = activeButton.ActualWidth;

                // Проверяем, видна ли кнопка полностью
                if (buttonPosition.X < 0)
                {
                    // Кнопка слева от видимой области - прокручиваем к началу
                    TabsScrollViewer.ScrollToLeftEnd();
                }
                else if (buttonPosition.X + buttonWidth > scrollViewerWidth)
                {
                    // Кнопка справа от видимой области - прокручиваем к концу
                    TabsScrollViewer.ScrollToRightEnd();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Ошибка при прокрутке к активной вкладке");
            }
        }

        /// <summary>
        /// Обработчик нажатия клавиш для навигации по вкладкам
        /// </summary>
        private void SettingsWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                // Ctrl + Tab - следующая вкладка
                if (e.Key == System.Windows.Input.Key.Tab &&
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    NavigateToNextTab();
                    e.Handled = true;
                }
                // Ctrl + Shift + Tab - предыдущая вкладка
                else if (e.Key == System.Windows.Input.Key.Tab &&
                         (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                         (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
                {
                    NavigateToPreviousTab();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Ошибка при обработке навигации по клавишам");
            }
        }

        /// <summary>
        /// Переходит к следующей вкладке
        /// </summary>
        private void NavigateToNextTab()
        {
            var tabs = new[] { MainTabButton, SizeTabButton, AnimationTabButton, StyleTabButton, PerformanceTabButton, AdvancedTabButton };
            var contents = new[] { MainTabContent, SizeTabContent, AnimationTabContent, StyleTabContent, PerformanceTabContent, AdvancedTabContent };

            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i].Background.ToString().Contains("#FF007AFF")) // Активная вкладка
                {
                    var nextIndex = (i + 1) % tabs.Length;
                    SetActiveTab(tabs[nextIndex], contents[nextIndex]);
                    break;
                }
            }
        }

        /// <summary>
        /// Переходит к предыдущей вкладке
        /// </summary>
        private void NavigateToPreviousTab()
        {
            var tabs = new[] { MainTabButton, SizeTabButton, AnimationTabButton, StyleTabButton, PerformanceTabButton, AdvancedTabButton };
            var contents = new[] { MainTabContent, SizeTabContent, AnimationTabContent, StyleTabContent, PerformanceTabContent, AdvancedTabContent };

            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i].Background.ToString().Contains("#FF007AFF")) // Активная вкладка
                {
                    var prevIndex = (i - 1 + tabs.Length) % tabs.Length;
                    SetActiveTab(tabs[prevIndex], contents[prevIndex]);
                    break;
                }
            }
        }
    }
}
