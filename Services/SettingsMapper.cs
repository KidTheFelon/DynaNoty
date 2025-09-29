using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DynaNoty.Configuration;
using DynaNoty.ViewModels;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Маппер для преобразования между UI элементами и настройками
    /// </summary>
    public class SettingsMapper
    {
        private readonly ILogger _logger;

        public SettingsMapper(ILogger logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Маппит значения из ViewModel в UI элементы
        /// </summary>
        public void MapViewModelToUI(SettingsViewModel viewModel, Dictionary<string, Control> uiElements)
        {
            try
            {
                // Основные настройки
                SetTextBoxValue(uiElements, "AutoHideTimeoutTextBox", viewModel.AutoHideTimeout.ToString());
                SetTextBoxValue(uiElements, "MaxNotificationsTextBox", viewModel.MaxNotifications.ToString());
                SetCheckBoxValue(uiElements, "EnableAnimationsCheckBox", viewModel.EnableAnimations);
                SetCheckBoxValue(uiElements, "ShowActionButtonCheckBox", viewModel.ShowActionButton);
                SetCheckBoxValue(uiElements, "AutoAdaptToSystemThemeCheckBox", viewModel.AutoAdaptToSystemTheme);
                SetCheckBoxValue(uiElements, "UseSystemAccentColorCheckBox", viewModel.UseSystemAccentColor);
                SetCheckBoxValue(uiElements, "SystemSettingsOverrideCheckBox", viewModel.SystemSettingsOverride);
                SetTextBoxValue(uiElements, "CleanupIntervalTextBox", viewModel.CleanupInterval.ToString());
                SetCheckBoxValue(uiElements, "ShowInSystemTrayCheckBox", viewModel.ShowInSystemTray);
                SetCheckBoxValue(uiElements, "EnableSoundCheckBox", viewModel.EnableSound);
                SetCheckBoxValue(uiElements, "EnableVibrationCheckBox", viewModel.EnableVibration);

                // Производительность
                SetTextBoxValue(uiElements, "MaxPoolSizeTextBox", viewModel.MaxPoolSize.ToString());
                SetTextBoxValue(uiElements, "PreWarmCountTextBox", viewModel.PreWarmCount.ToString());
                SetCheckBoxValue(uiElements, "EnableCachingCheckBox", viewModel.EnableCaching);
                SetTextBoxValue(uiElements, "MaxCacheSizeTextBox", viewModel.MaxCacheSize.ToString());

                // Размеры и позиционирование
                SetTextBoxValue(uiElements, "MaxWidthTextBox", viewModel.MaxWidth.ToString());
                SetTextBoxValue(uiElements, "MinWidthTextBox", viewModel.MinWidth.ToString());
                SetTextBoxValue(uiElements, "HeightTextBox", viewModel.Height.ToString());
                SetTextBoxValue(uiElements, "ExpandedHeightTextBox", viewModel.ExpandedHeight.ToString());
                SetTextBoxValue(uiElements, "TopMarginTextBox", viewModel.TopMargin.ToString());
                SetTextBoxValue(uiElements, "VerticalSpacingTextBox", viewModel.VerticalSpacing.ToString());
                SetTextBoxValue(uiElements, "CornerRadiusTextBox", viewModel.CornerRadius.ToString());
                SetTextBoxValue(uiElements, "NotificationAreaHeightTextBox", viewModel.NotificationAreaHeight.ToString());
                SetTextBoxValue(uiElements, "IconSizeTextBox", viewModel.IconSize.ToString());
                SetTextBoxValue(uiElements, "ActionButtonSizeTextBox", viewModel.ActionButtonSize.ToString());
                SetTextBoxValue(uiElements, "MaxHeightTextBox", viewModel.MaxHeight.ToString());
                SetTextBoxValue(uiElements, "FullyExpandedMinHeightTextBox", viewModel.FullyExpandedMinHeight.ToString());
                SetTextBoxValue(uiElements, "FullyExpandedBaseHeightTextBox", viewModel.FullyExpandedBaseHeight.ToString());
                SetTextBoxValue(uiElements, "ActionButtonHeightPercentTextBox", viewModel.ActionButtonHeightPercent.ToString());
                SetTextBoxValue(uiElements, "ActionButtonMinHeightTextBox", viewModel.ActionButtonMinHeight.ToString());
                SetTextBoxValue(uiElements, "ActionButtonMaxHeightTextBox", viewModel.ActionButtonMaxHeight.ToString());
                SetTextBoxValue(uiElements, "ActionButtonFontScaleTextBox", viewModel.ActionButtonFontScale.ToString());

                // Анимации
                SetTextBoxValue(uiElements, "AppearDurationTextBox", viewModel.AppearDuration.ToString());
                SetTextBoxValue(uiElements, "ExpandDurationTextBox", viewModel.ExpandDuration.ToString());
                SetTextBoxValue(uiElements, "RepositionDurationTextBox", viewModel.RepositionDuration.ToString());
                SetTextBoxValue(uiElements, "ExpandDelayTextBox", viewModel.ExpandDelay.ToString());
                SetTextBoxValue(uiElements, "CompactDisplayDurationTextBox", viewModel.CompactDisplayDuration.ToString());
                SetTextBoxValue(uiElements, "ExpandedDisplayDurationTextBox", viewModel.ExpandedDisplayDuration.ToString());
                SetTextBoxValue(uiElements, "FullyExpandedDisplayDurationTextBox", viewModel.FullyExpandedDisplayDuration.ToString());
                SetCheckBoxValue(uiElements, "EnableAutoExpandCheckBox", viewModel.EnableAutoExpand);
                SetComboBoxValue(uiElements, "PhysicsPresetComboBox", viewModel.PhysicsPreset);
                SetCheckBoxValue(uiElements, "UsePhysicsForRepositionCheckBox", viewModel.UsePhysicsForReposition);

                // Стили
                SetColorPickerValue(uiElements, "BackgroundColorPicker", viewModel.BackgroundColor);
                SetColorPickerValue(uiElements, "TextColorPicker", viewModel.TextColor);
                SetColorPickerValue(uiElements, "IconColorPicker", viewModel.IconColor);
                SetTextBoxValue(uiElements, "TitleFontSizeTextBox", viewModel.TitleFontSize.ToString());
                SetTextBoxValue(uiElements, "SubtitleFontSizeTextBox", viewModel.SubtitleFontSize.ToString());
                SetTextBoxValue(uiElements, "IconFontSizeTextBox", viewModel.IconFontSize.ToString());

                // Логирование
                SetCheckBoxValue(uiElements, "EnableLoggingCheckBox", viewModel.EnableLogging);
                SetComboBoxValue(uiElements, "LogLevelComboBox", viewModel.LogLevel);

                // Тема
                SetComboBoxValue(uiElements, "ThemeComboBox", viewModel.Theme);

                _logger?.LogDebug("Значения ViewModel успешно мапплены в UI элементы");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка маппинга ViewModel в UI элементы");
                throw;
            }
        }

        /// <summary>
        /// Маппит значения из UI элементов в ViewModel
        /// </summary>
        public void MapUIToViewModel(Dictionary<string, Control> uiElements, SettingsViewModel viewModel)
        {
            try
            {
                // Основные настройки
                viewModel.AutoHideTimeout = GetIntValue(uiElements, "AutoHideTimeoutTextBox");
                viewModel.MaxNotifications = GetIntValue(uiElements, "MaxNotificationsTextBox");
                viewModel.EnableAnimations = GetCheckBoxValue(uiElements, "EnableAnimationsCheckBox");
                viewModel.ShowActionButton = GetCheckBoxValue(uiElements, "ShowActionButtonCheckBox");
                viewModel.AutoAdaptToSystemTheme = GetCheckBoxValue(uiElements, "AutoAdaptToSystemThemeCheckBox");
                viewModel.UseSystemAccentColor = GetCheckBoxValue(uiElements, "UseSystemAccentColorCheckBox");
                viewModel.SystemSettingsOverride = GetCheckBoxValue(uiElements, "SystemSettingsOverrideCheckBox");
                viewModel.CleanupInterval = GetIntValue(uiElements, "CleanupIntervalTextBox");
                viewModel.ShowInSystemTray = GetCheckBoxValue(uiElements, "ShowInSystemTrayCheckBox");
                viewModel.EnableSound = GetCheckBoxValue(uiElements, "EnableSoundCheckBox");
                viewModel.EnableVibration = GetCheckBoxValue(uiElements, "EnableVibrationCheckBox");

                // Производительность
                viewModel.MaxPoolSize = GetIntValue(uiElements, "MaxPoolSizeTextBox");
                viewModel.PreWarmCount = GetIntValue(uiElements, "PreWarmCountTextBox");
                viewModel.EnableCaching = GetCheckBoxValue(uiElements, "EnableCachingCheckBox");
                viewModel.MaxCacheSize = GetIntValue(uiElements, "MaxCacheSizeTextBox");

                // Размеры и позиционирование
                viewModel.MaxWidth = GetDoubleValue(uiElements, "MaxWidthTextBox");
                viewModel.MinWidth = GetDoubleValue(uiElements, "MinWidthTextBox");
                viewModel.Height = GetDoubleValue(uiElements, "HeightTextBox");
                viewModel.ExpandedHeight = GetDoubleValue(uiElements, "ExpandedHeightTextBox");
                viewModel.TopMargin = GetDoubleValue(uiElements, "TopMarginTextBox");
                viewModel.VerticalSpacing = GetDoubleValue(uiElements, "VerticalSpacingTextBox");
                viewModel.CornerRadius = GetDoubleValue(uiElements, "CornerRadiusTextBox");
                viewModel.NotificationAreaHeight = GetDoubleValue(uiElements, "NotificationAreaHeightTextBox");
                viewModel.IconSize = GetDoubleValue(uiElements, "IconSizeTextBox");
                viewModel.ActionButtonSize = GetDoubleValue(uiElements, "ActionButtonSizeTextBox");
                viewModel.MaxHeight = GetDoubleValue(uiElements, "MaxHeightTextBox");
                viewModel.FullyExpandedMinHeight = GetDoubleValue(uiElements, "FullyExpandedMinHeightTextBox");
                viewModel.FullyExpandedBaseHeight = GetDoubleValue(uiElements, "FullyExpandedBaseHeightTextBox");
                viewModel.ActionButtonHeightPercent = GetDoubleValue(uiElements, "ActionButtonHeightPercentTextBox");
                viewModel.ActionButtonMinHeight = GetDoubleValue(uiElements, "ActionButtonMinHeightTextBox");
                viewModel.ActionButtonMaxHeight = GetDoubleValue(uiElements, "ActionButtonMaxHeightTextBox");
                viewModel.ActionButtonFontScale = GetDoubleValue(uiElements, "ActionButtonFontScaleTextBox");

                // Анимации
                viewModel.AppearDuration = GetIntValue(uiElements, "AppearDurationTextBox");
                viewModel.ExpandDuration = GetIntValue(uiElements, "ExpandDurationTextBox");
                viewModel.RepositionDuration = GetIntValue(uiElements, "RepositionDurationTextBox");
                viewModel.ExpandDelay = GetIntValue(uiElements, "ExpandDelayTextBox");
                viewModel.CompactDisplayDuration = GetIntValue(uiElements, "CompactDisplayDurationTextBox");
                viewModel.ExpandedDisplayDuration = GetIntValue(uiElements, "ExpandedDisplayDurationTextBox");
                viewModel.FullyExpandedDisplayDuration = GetIntValue(uiElements, "FullyExpandedDisplayDurationTextBox");
                viewModel.EnableAutoExpand = GetCheckBoxValue(uiElements, "EnableAutoExpandCheckBox");
                viewModel.PhysicsPreset = GetComboBoxValue(uiElements, "PhysicsPresetComboBox");
                viewModel.UsePhysicsForReposition = GetCheckBoxValue(uiElements, "UsePhysicsForRepositionCheckBox");

                // Стили
                viewModel.BackgroundColor = GetColorPickerValue(uiElements, "BackgroundColorPicker");
                viewModel.TextColor = GetColorPickerValue(uiElements, "TextColorPicker");
                viewModel.IconColor = GetColorPickerValue(uiElements, "IconColorPicker");
                viewModel.TitleFontSize = GetDoubleValue(uiElements, "TitleFontSizeTextBox");
                viewModel.SubtitleFontSize = GetDoubleValue(uiElements, "SubtitleFontSizeTextBox");
                viewModel.IconFontSize = GetDoubleValue(uiElements, "IconFontSizeTextBox");

                // Логирование
                viewModel.EnableLogging = GetCheckBoxValue(uiElements, "EnableLoggingCheckBox");
                viewModel.LogLevel = GetComboBoxValue(uiElements, "LogLevelComboBox");

                // Тема
                viewModel.Theme = GetComboBoxValue(uiElements, "ThemeComboBox");

                _logger?.LogDebug("Значения UI элементов успешно мапплены в ViewModel");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка маппинга UI элементов в ViewModel");
                throw;
            }
        }

        #region Helper Methods

        private void SetTextBoxValue(Dictionary<string, Control> uiElements, string key, string value)
        {
            if (uiElements.TryGetValue(key, out var control) && control is TextBox textBox)
            {
                textBox.Text = value;
            }
        }

        private void SetCheckBoxValue(Dictionary<string, Control> uiElements, string key, bool value)
        {
            if (uiElements.TryGetValue(key, out var control) && control is CheckBox checkBox)
            {
                checkBox.IsChecked = value;
            }
        }

        private void SetComboBoxValue(Dictionary<string, Control> uiElements, string key, int value)
        {
            if (uiElements.TryGetValue(key, out var control) && control is ComboBox comboBox)
            {
                comboBox.SelectedIndex = value;
            }
        }

        private void SetColorPickerValue(Dictionary<string, Control> uiElements, string key, Color value)
        {
            if (uiElements.TryGetValue(key, out var control) && control is Xceed.Wpf.Toolkit.ColorPicker colorPicker)
            {
                colorPicker.SelectedColor = value;
            }
        }

        private int GetIntValue(Dictionary<string, Control> uiElements, string key)
        {
            if (uiElements.TryGetValue(key, out var control) && control is TextBox textBox)
            {
                return int.TryParse(textBox.Text, out int value) ? value : 0;
            }
            return 0;
        }

        private double GetDoubleValue(Dictionary<string, Control> uiElements, string key)
        {
            if (uiElements.TryGetValue(key, out var control) && control is TextBox textBox)
            {
                return double.TryParse(textBox.Text, out double value) ? value : 0.0;
            }
            return 0.0;
        }

        private bool GetCheckBoxValue(Dictionary<string, Control> uiElements, string key)
        {
            if (uiElements.TryGetValue(key, out var control) && control is CheckBox checkBox)
            {
                return checkBox.IsChecked ?? false;
            }
            return false;
        }

        private int GetComboBoxValue(Dictionary<string, Control> uiElements, string key)
        {
            if (uiElements.TryGetValue(key, out var control) && control is ComboBox comboBox)
            {
                return comboBox.SelectedIndex;
            }
            return 0;
        }

        private Color GetColorPickerValue(Dictionary<string, Control> uiElements, string key)
        {
            if (uiElements.TryGetValue(key, out var control) && control is Xceed.Wpf.Toolkit.ColorPicker colorPicker)
            {
                return colorPicker.SelectedColor ?? Colors.Black;
            }
            return Colors.Black;
        }

        #endregion
    }
}
