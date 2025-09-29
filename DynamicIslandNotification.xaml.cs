using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Threading;
using DynaNoty.Configuration;
using DynaNoty.Services;
using DynaNoty.Interfaces;
using DynaNoty.Models;
using DynaNoty.Constants;
using Microsoft.Extensions.Logging;

namespace DynaNoty
{

    public partial class DynamicIslandNotification : UserControl, IDisposable
    {
        public event EventHandler Dismissed;
        public event EventHandler ActionClicked;
        public event EventHandler<NotificationActionEventArgs> CustomActionClicked;

        private bool _disposed = false;
        private NotificationConfiguration _config;
        private NotificationStateManager _stateManager;
        private NotificationUIManager _uiManager;
        private NotificationAnimationService _animationService;
        private INotificationPositioningService _positioningService;
        private ILogger<DynamicIslandNotification> _logger;
        private int _notificationIndex = 0;

        /// <summary>
        /// Проверяет, удален ли объект
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Цвет текста уведомления
        /// </summary>
        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register("TextColor", typeof(Brush), typeof(DynamicIslandNotification),
                new PropertyMetadata(Brushes.White));

        public DynamicIslandNotification(NotificationConfiguration config = null, ISystemThemeService themeService = null, ILogger<DynamicIslandNotification> logger = null)
        {
            _disposed = false;
            InitializeComponent();
            _config = config ?? new NotificationConfiguration();
            _logger = logger;

            var systemThemeService = themeService ?? new SystemThemeService();
            _stateManager = new NotificationStateManager(_config, logger);
            _uiManager = new NotificationUIManager(_config, systemThemeService, logger);
            // Создаем логгер для NotificationAnimationService из логгера DynamicIslandNotification
            var animationLogger = _logger != null ?
                Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<NotificationAnimationService>() :
                null;

            _animationService = new NotificationAnimationService(_config, animationLogger);

            // Отладочное логирование для проверки передачи логгера
            System.Diagnostics.Debug.WriteLine($"DynamicIslandNotification создан. Logger: {(_logger != null ? "НЕ NULL" : "NULL")}, AnimationService logger: {(_animationService.Logger != null ? "НЕ NULL" : "NULL")}");

            SetupEventHandlers(systemThemeService);
        }

        /// <summary>
        /// Настраивает обработчики событий
        /// </summary>
        private void SetupEventHandlers(ISystemThemeService themeService)
        {
            // Подписываемся на изменения системной темы
            themeService.SystemThemeChanged += OnSystemThemeChanged;

            // Добавляем обработчик клика для раскрытия
            this.MouseLeftButtonDown += OnNotificationClick;

            // Подписываемся на события менеджера состояний
            _stateManager.StateChanged += OnStateChanged;
            _stateManager.AutoHideTriggered += OnAutoHideTriggered;

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Инициализация завершена, анимация будет запущена в ShowNotification
        }

        /// <summary>
        /// Устанавливает позиционирующий сервис и индекс уведомления
        /// </summary>
        public void SetPositioningService(INotificationPositioningService positioningService, int index)
        {
            _positioningService = positioningService;
            _notificationIndex = index;
        }

        /// <summary>
        /// Обработчик изменения системной темы
        /// </summary>
        private void OnSystemThemeChanged(object sender, System.EventArgs e)
        {
            if (_disposed) return;

            // Применяем новые цвета в UI потоке
            if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                Dispatcher.Invoke(() => ApplyColors());
            }
            else
            {
                ApplyColors();
            }
        }

        /// <summary>
        /// Проверяет, развернуто ли уведомление
        /// </summary>
        public bool IsExpanded => _stateManager.CurrentState != NotificationState.Compact;

        /// <summary>
        /// Обработчик клика по уведомлению для раскрытия
        /// </summary>
        private void OnNotificationClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_disposed) return;

            try
            {
                _logger?.LogDebug("Клик по уведомлению. Текущее состояние: {State}", _stateManager.CurrentState);

                if (_stateManager.CurrentState == NotificationState.Compact)
                {
                    HandleCompactStateClick();
                }
                else if (_stateManager.CurrentState == NotificationState.Expanded)
                {
                    HandleExpandedStateClick();
                }
                else if (_stateManager.CurrentState == NotificationState.FullyExpanded)
                {
                    HandleFullyExpandedStateClick();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при обработке клика по уведомлению");
            }
        }

        private void HandleCompactStateClick()
        {
            _stateManager.ChangeState(NotificationState.Expanded);
            _uiManager.SetTextDisplayMode(TitleText, SubText, true, false); // Первое раскрытие - одна строка с троеточиями

            _stateManager.StopAutoHideTimer();
            _stateManager.StopExpandTimer();

            _animationService.AnimateExpand(MainBorder, ContentPanel, () =>
            {
                if (!_disposed)
                {
                    try
                    {
                        _logger?.LogDebug("Анимация расширения завершена, запускаем таймер автоскрытия для расширенного состояния");
                        // Запускаем таймер автоскрытия для расширенного состояния
                        _stateManager.StartAutoHideTimer();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ошибка после анимации расширения");
                    }
                }
                else
                {
                    _logger?.LogDebug("Уведомление уже удалено");
                }
            }, ActionButton, IconContainer, this);
        }

        private void HandleExpandedStateClick()
        {
            // При клике по расширенному уведомлению переходим к полному раскрытию
            _logger?.LogDebug("Клик по расширенному уведомлению - переходим к полному раскрытию. EnableAnimations: {EnableAnimations}, ExpandAnimationDuration: {Duration}ms",
                _config.EnableAnimations, _config.ExpandAnimationDuration);
            ForceExpand();
        }

        private void HandleFullyExpandedStateClick()
        {
            // При клике по полностью раскрытому уведомлению закрываем его
            _logger?.LogDebug("Клик по полностью раскрытому уведомлению - закрываем");
            AnimateDismiss();
        }

        /// <summary>
        /// Обработчик изменения состояния
        /// </summary>
        private void OnStateChanged(object sender, EventArgs e)
        {
            if (_stateManager.CurrentState == NotificationState.Expanded)
            {
                OnExpandTriggered();
            }
        }

        /// <summary>
        /// Обработчик срабатывания таймера автоскрытия
        /// </summary>
        private void OnAutoHideTriggered(object sender, EventArgs e)
        {
            AnimateDismiss();
        }

        /// <summary>
        /// Принудительно раскрывает уведомление
        /// </summary>
        private void ForceExpand()
        {
            if (_disposed || _stateManager.CurrentState == NotificationState.FullyExpanded) return;

            try
            {
                _logger?.LogDebug("Принудительное раскрытие уведомления. Текущее состояние: {State}, EnableAnimations: {EnableAnimations}",
                    _stateManager.CurrentState, _config.EnableAnimations);

                _stateManager.ChangeState(NotificationState.FullyExpanded);

                // Устанавливаем режим отображения текста для полностью раскрытого состояния (полный текст)
                _uiManager.SetTextDisplayMode(TitleText, SubText, true, true);

                // Обновляем и показываем действия при полном раскрытии
                _uiManager.UpdateActionsPanel(MainBorder, ActionsPanel, OnActionButtonClick);

                _animationService.AnimateFullyExpand(MainBorder, ContentPanel, () =>
                {
                    if (!_disposed)
                    {
                        try
                        {
                            // Устанавливаем финальные размеры после завершения анимации
                            _uiManager.SetFullyExpandedSize(MainBorder, ContentPanel, ActionButton, IconContainer);
                            _logger?.LogDebug("Принудительное раскрытие завершено, запускаем таймер автоскрытия для полностью раскрытого состояния");
                            // Запускаем таймер автоскрытия для полностью раскрытого состояния
                            _stateManager.StartAutoHideTimer();
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Ошибка после принудительного раскрытия");
                        }
                    }
                }, ActionButton, this);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка принудительного раскрытия");
            }
        }

        /// <summary>
        /// Применяет цвета с учетом системных настроек
        /// </summary>
        private void ApplyColors()
        {
            // Оптимизированная проверка UI потока
            if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                Dispatcher.Invoke(() => ApplyColors());
                return;
            }

            _uiManager.ApplyColors(MainBorder, TitleText, SubText, IconText, IconContainer, ActionButton);
        }

        public void ShowNotification(string title, string subtitle, string icon = null, bool showAction = true, List<NotificationAction> actions = null)
        {
            ThrowIfDisposed();

            // Оптимизированная проверка UI потока
            if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                Dispatcher.Invoke(() => ShowNotification(title, subtitle, icon, showAction, actions));
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"DynamicIslandNotification.ShowNotification вызван: {title} - {subtitle}");
                _logger?.LogDebug("Показываем уведомление: {Title} - {Subtitle}", title, subtitle);

                System.Diagnostics.Debug.WriteLine("Настраиваем содержимое уведомления");
                // Настраиваем содержимое
                _uiManager.SetupContent(TitleText, SubText, IconText, title, subtitle, icon);
                System.Diagnostics.Debug.WriteLine("Содержимое уведомления настроено");

                System.Diagnostics.Debug.WriteLine("Устанавливаем режим отображения текста");
                // Устанавливаем режим отображения текста для свернутого состояния
                _uiManager.SetTextDisplayMode(TitleText, SubText, false, false);
                System.Diagnostics.Debug.WriteLine("Режим отображения текста установлен");

                System.Diagnostics.Debug.WriteLine("Настраиваем действия");
                // Настраиваем действия
                _uiManager.SetupActions(actions);
                _uiManager.UpdateActionsPanel(MainBorder, ActionsPanel, OnActionButtonClick);
                System.Diagnostics.Debug.WriteLine("Действия настроены");

                System.Diagnostics.Debug.WriteLine("Применяем цвета");
                // Применяем цвета
                ApplyColors();
                System.Diagnostics.Debug.WriteLine("Цвета применены");

                System.Diagnostics.Debug.WriteLine("Устанавливаем начальное состояние");
                // Устанавливаем начальное состояние в компактном виде
                this.Visibility = Visibility.Visible;
                this.Opacity = 0.0;
                System.Diagnostics.Debug.WriteLine("Начальное состояние установлено");

                System.Diagnostics.Debug.WriteLine("Устанавливаем компактный размер");
                // Устанавливаем компактный размер
                _uiManager.SetCompactSize(MainBorder, this, ContentPanel, ActionButton, IconContainer);
                _stateManager.ChangeState(NotificationState.Compact);
                System.Diagnostics.Debug.WriteLine("Компактный размер установлен");

                System.Diagnostics.Debug.WriteLine("Пересчитываем позицию");
                // Пересчитываем позицию после установки компактного размера
                // Используем низкий приоритет для оптимизации производительности
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (!_disposed && _positioningService != null)
                        {
                            _positioningService.RecalculatePositionAfterResize(this, _notificationIndex);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ошибка пересчета позиции после установки компактного размера");
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
                System.Diagnostics.Debug.WriteLine("Позиция пересчитана");

                System.Diagnostics.Debug.WriteLine("Запускаем анимацию появления");
                // Запускаем анимацию появления
                _animationService.AnimateAppear(this);
                System.Diagnostics.Debug.WriteLine("Анимация появления запущена");

                System.Diagnostics.Debug.WriteLine("Запускаем таймеры");
                // Запускаем таймеры
                System.Diagnostics.Debug.WriteLine($"Проверяем EnableAutoExpand: {_config.EnableAutoExpand}, ExpandDelay: {_config.ExpandDelay}ms");
                _logger?.LogInformation("Проверяем EnableAutoExpand: {EnableAutoExpand}, ExpandDelay: {ExpandDelay}ms",
                    _config.EnableAutoExpand, _config.ExpandDelay);

                if (_config.EnableAutoExpand)
                {
                    System.Diagnostics.Debug.WriteLine("Запускаем таймер расширения");
                    _logger?.LogInformation("Запускаем таймер расширения");
                    _stateManager.StartExpandTimer();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Автораскрытие отключено, таймер расширения не запускаем");
                    _logger?.LogInformation("Автораскрытие отключено, таймер расширения не запускаем");
                }
                // НЕ запускаем таймер автоскрытия для компактного состояния
                // Уведомление остается компактным до клика пользователя

                System.Diagnostics.Debug.WriteLine($"Уведомление настроено. AutoExpand: {_config.EnableAutoExpand}, ExpandDelay: {_config.ExpandDelay}ms");
                _logger?.LogInformation("Уведомление настроено. AutoExpand: {EnableAutoExpand}, ExpandDelay: {ExpandDelay}ms",
                    _config.EnableAutoExpand, _config.ExpandDelay);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка в ShowNotification");
                throw;
            }
        }

        public void ShowCompact(string icon = null)
        {
            ThrowIfDisposed();

            // Оптимизированная проверка UI потока
            if (Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                Dispatcher.Invoke(() => ShowCompact(icon));
                return;
            }

            IconText.Text = icon ?? NotificationConstants.DEFAULT_ICON;
            _uiManager.SetCompactSize(MainBorder, this, ContentPanel, ActionButton, IconContainer);
            _stateManager.ChangeState(NotificationState.Compact);

            // Пересчитываем позицию после установки компактного размера
            // Используем низкий приоритет для оптимизации производительности
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (!_disposed && _positioningService != null)
                    {
                        _positioningService.RecalculatePositionAfterResize(this, _notificationIndex);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка пересчета позиции в ShowCompact");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

            // Применяем цвета
            ApplyColors();
        }

        /// <summary>
        /// Обработчик расширения уведомления
        /// </summary>
        private void OnExpandTriggered()
        {
            if (_disposed) return;

            _logger?.LogDebug("Таймер расширения сработал");

            _stateManager.ChangeState(NotificationState.Expanded);

            // Устанавливаем режим отображения текста для расширенного состояния (одна строка с троеточиями)
            _uiManager.SetTextDisplayMode(TitleText, SubText, true, false);

            _logger?.LogDebug("Запускаем анимацию расширения по ширине");

            _animationService.AnimateExpand(MainBorder, ContentPanel, () =>
            {
                if (!_disposed)
                {
                    try
                    {
                        _logger?.LogDebug("Анимация расширения завершена, запускаем таймер автоскрытия для расширенного состояния");
                        // Запускаем таймер автоскрытия для расширенного состояния
                        _stateManager.StartAutoHideTimer();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ошибка после анимации расширения");
                    }
                }
                else
                {
                    _logger?.LogDebug("Уведомление уже удалено");
                }
            }, ActionButton, IconContainer, this);
        }

        /// <summary>
        /// Обработчик клика по кнопке действия
        /// </summary>
        private void OnActionButtonClick(object sender, NotificationActionEventArgs e)
        {
            try
            {
                _logger?.LogDebug("Клик по действию: {ActionId} - {ActionText}", e.ActionId, e.ActionText);
                CustomActionClicked?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при обработке действия");
            }
        }

        /// <summary>
        /// Проверяет, не удален ли объект
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DynamicIslandNotification));
        }




        /// <summary>
        /// Анимация исчезновения уведомления
        /// </summary>
        private void AnimateDismiss()
        {
            _logger?.LogDebug("Запускаем анимацию исчезновения");

            try
            {
                _animationService.AnimateDismiss(this, () =>
                {
                    if (!_disposed)
                    {
                        _logger?.LogDebug("Анимация исчезновения завершена, вызываем событие Dismissed");
                        Dismissed?.Invoke(this, EventArgs.Empty);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка в анимации исчезновения");
                // В случае ошибки анимации все равно вызываем событие закрытия
                if (!_disposed)
                {
                    Dismissed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки действия
        /// </summary>
        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ThrowIfDisposed();

            ActionClicked?.Invoke(this, EventArgs.Empty);
            AnimateDismiss();
        }

        /// <summary>
        /// Принудительно закрывает уведомление
        /// </summary>
        public void Dismiss()
        {
            ThrowIfDisposed();

            _stateManager.StopAutoHideTimer();
            AnimateDismiss();
        }

        /// <summary>
        /// Освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _logger?.LogDebug("Dispose вызван для уведомления");
                _disposed = true;

                try
                {
                    // Останавливаем все таймеры
                    _stateManager?.StopAllTimers();
                    _stateManager?.Dispose();

                    // Очищаем UI менеджер
                    _uiManager?.Dispose();

                    // Очищаем анимационный сервис
                    _animationService?.Dispose();

                    // Отписываемся от системной темы
                    if (_config?.AutoAdaptToSystemTheme == true)
                    {
                        var themeService = new SystemThemeService();
                        themeService.SystemThemeChanged -= OnSystemThemeChanged;
                    }

                    // Отписываемся от событий UI
                    this.MouseLeftButtonDown -= OnNotificationClick;
                    this.Loaded -= OnLoaded;

                    // Безопасно очищаем события
                    ClearEvents();

                    // Очищаем ссылки
                    _positioningService = null;
                    _config = null;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка при освобождении ресурсов уведомления");
                }
            }
            else
            {
                _logger?.LogDebug("Dispose уже был вызван для уведомления");
            }
        }

        /// <summary>
        /// Безопасно очищает все события
        /// </summary>
        private void ClearEvents()
        {
            try
            {
                // Отписываемся от всех подписчиков для предотвращения утечек памяти
                Dismissed = null;
                ActionClicked = null;
                CustomActionClicked = null;

                _logger?.LogDebug("События уведомления очищены");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка очистки событий");
            }
        }
    }
}
