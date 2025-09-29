using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DynaNoty.Configuration;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для управления анимациями уведомлений
    /// </summary>
    public class NotificationAnimationService
    {
        private readonly NotificationConfiguration _config;
        private readonly AnimationFactory _animationFactory;
        private readonly ILogger _logger;
        
        // Публичное свойство для отладки
        public ILogger Logger => _logger;
        private bool _disposed = false;

        public NotificationAnimationService(NotificationConfiguration config, ILogger<NotificationAnimationService> logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
            _animationFactory = new AnimationFactory(config, logger);
            
            // Отладочное логирование для проверки инициализации
            System.Diagnostics.Debug.WriteLine($"NotificationAnimationService создан. Logger: {(_logger != null ? "НЕ NULL" : "NULL")}");
        }

        /// <summary>
        /// Анимация появления уведомления
        /// </summary>
        public void AnimateAppear(UserControl notification)
        {
            if (_disposed) return;
            
            _logger?.LogDebug("AnimateAppear вызван. EnableAnimations: {EnableAnimations}", _config.EnableAnimations);
            
            if (!_config.EnableAnimations)
            {
                notification.Opacity = 1.0;
                _logger?.LogDebug("Анимации отключены, устанавливаем Opacity = 1.0");
                return;
            }

            // Останавливаем все активные анимации для предотвращения конфликтов
            StopAllAnimations(notification);

            // Настраиваем трансформацию
            var scaleTransform = new ScaleTransform(0.8, 0.8);
            notification.RenderTransform = scaleTransform;
            notification.RenderTransformOrigin = new Point(0.5, 0.5);

            // Создаем анимации
            var animations = _animationFactory.CreateAppearAnimations();

            // Добавляем обработчик завершения анимации прозрачности
            animations.Opacity.Completed += (s, e) => 
            {
                _logger?.LogDebug("Анимация появления завершена");
                // Устанавливаем финальные значения для предотвращения проблем с отображением
                notification.Opacity = 1.0;
            };

            // Применяем анимации
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animations.ScaleX);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animations.ScaleY);
            notification.BeginAnimation(UIElement.OpacityProperty, animations.Opacity);

            _logger?.LogDebug("Анимация появления запущена. Duration: {Duration}ms", _config.AppearAnimationDuration);
        }

        /// <summary>
        /// Анимация расширения уведомления
        /// </summary>
        public void AnimateExpand(Border mainBorder, Panel contentPanel, Action onCompleted = null, UIElement actionButton = null, FrameworkElement iconContainer = null, UserControl notification = null)
        {
            _logger?.LogInformation("=== НАЧАЛО АНИМАЦИИ ПЕРВОГО РАСКРЫТИЯ ===");
            _logger?.LogInformation("AnimateExpand вызван. EnableAnimations: {EnableAnimations}", _config.EnableAnimations);
            
            if (_disposed) 
            {
                _logger?.LogWarning("Сервис анимации уже освобожден, пропускаем анимацию");
                return;
            }
            
            if (!_config.EnableAnimations)
            {
                _logger?.LogInformation("Анимации отключены, устанавливаем размеры напрямую");
                mainBorder.Width = _config.MaxNotificationWidth;
                contentPanel.Visibility = Visibility.Visible;
                contentPanel.Opacity = 1.0;
                if (actionButton != null)
                {
                    actionButton.Visibility = Visibility.Visible;
                    actionButton.Opacity = 1.0;
                }
                onCompleted?.Invoke();
                return;
            }

            // Останавливаем все активные анимации для предотвращения конфликтов
            _logger?.LogDebug("Останавливаем все активные анимации");
            StopAllAnimations(mainBorder);
            contentPanel.BeginAnimation(UIElement.OpacityProperty, null);
            if (actionButton != null)
            {
                actionButton.BeginAnimation(UIElement.OpacityProperty, null);
            }
            _logger?.LogDebug("Анимации остановлены");

            // Показываем контент (но с нулевой прозрачностью для анимации)
            contentPanel.Visibility = Visibility.Visible;
            contentPanel.Opacity = 0.0;
            _logger?.LogDebug("Контент показан с нулевой прозрачностью");
            
            // Показываем кнопку действия (но с нулевой прозрачностью для анимации)
            if (actionButton != null)
            {
                actionButton.Visibility = Visibility.Visible;
                actionButton.Opacity = 0.0;
                _logger?.LogDebug("Кнопка действия показана с нулевой прозрачностью");
                System.Diagnostics.Debug.WriteLine($"Инициализация кнопки: Visibility={actionButton.Visibility}, Opacity={actionButton.Opacity}");
                System.Diagnostics.Debug.WriteLine($"Кнопка Grid.Column: {Grid.GetColumn(actionButton)}");
                
                if (actionButton is FrameworkElement fe)
                {
                    System.Diagnostics.Debug.WriteLine($"Кнопка размеры: Width={fe.Width}, Height={fe.Height}, ActualWidth={fe.ActualWidth}, ActualHeight={fe.ActualHeight}");
                }
            }
            
            // Перемещаем иконку в левую колонку для расширенного состояния
            if (iconContainer != null)
            {
                Grid.SetColumn(iconContainer, 0);
                _logger?.LogDebug("Иконка перемещена в левую колонку");
            }

            // Создаем анимацию позиции для плавного центрирования
            DoubleAnimation leftAnimation = null;
            if (notification != null)
            {
                var currentLeft = Canvas.GetLeft(notification);
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var targetLeft = (screenWidth - _config.MaxNotificationWidth) / 2;
                _logger?.LogInformation("Позиционирование: CurrentLeft={CurrentLeft}, TargetLeft={TargetLeft}, ScreenWidth={ScreenWidth}", 
                    currentLeft, targetLeft, screenWidth);
                
                // Анимируем позицию только если она изменится
                if (Math.Abs(currentLeft - targetLeft) > 1)
                {
                    leftAnimation = new DoubleAnimation(
                        currentLeft, 
                        targetLeft, 
                        TimeSpan.FromMilliseconds(_config.ExpandAnimationDuration))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    _logger?.LogDebug("Создана анимация позиции: от {CurrentLeft} до {TargetLeft}", currentLeft, targetLeft);
                }
                else
                {
                    _logger?.LogDebug("Позиция не изменилась, пропускаем анимацию позиции");
                }
            }

            // Создаем все анимации
            var widthAnimation = CreateWidthAnimation(
                _config.MinNotificationWidth, 
                _config.MaxNotificationWidth, 
                _config.ExpandAnimationDuration);
            _logger?.LogInformation("Создана анимация ширины: от {MinWidth} до {MaxWidth} за {Duration}ms", 
                _config.MinNotificationWidth, _config.MaxNotificationWidth, _config.ExpandAnimationDuration);
            
            var currentHeight = mainBorder.ActualHeight;
            if (currentHeight < _config.MinNotificationHeight)
            {
                currentHeight = _config.MinNotificationHeight;
            }
            
            var targetHeight = _config.ExpandedNotificationHeight;
            var heightAnimation = CreateHeightAnimation(currentHeight, targetHeight, (int)_config.ExpandAnimationDuration);
            _logger?.LogInformation("Создана анимация высоты: от {CurrentHeight} до {TargetHeight} за {Duration}ms", 
                currentHeight, targetHeight, _config.ExpandAnimationDuration);
            
            var contentOpacityAnimation = CreateOpacityAnimation(0, 1, _config.ExpandAnimationDuration);
            contentOpacityAnimation.BeginTime = TimeSpan.FromMilliseconds(200);
            _logger?.LogDebug("Создана анимация прозрачности контента: от 0 до 1 с задержкой 200ms");
            
            // Создаем анимацию для кнопки действия
            DoubleAnimation actionButtonOpacityAnimation = null;
            if (actionButton != null)
            {
                actionButtonOpacityAnimation = CreateOpacityAnimation(0, 1, _config.ExpandAnimationDuration);
                actionButtonOpacityAnimation.BeginTime = TimeSpan.FromMilliseconds(300);
                _logger?.LogDebug("Создана анимация кнопки действия: от 0 до 1 с задержкой 300ms");
                System.Diagnostics.Debug.WriteLine($"Создана анимация кнопки действия: Visibility={actionButton.Visibility}, Opacity={actionButton.Opacity}");
            }

            // Синхронизируем завершение всех анимаций
            var completedAnimations = 0;
            var totalAnimations = actionButton != null ? 5 : 4; // ширина + высота + контент + позиция + (кнопка)
            _logger?.LogInformation("Ожидаем завершения {TotalAnimations} анимаций", totalAnimations);
            
            Action checkCompletion = () =>
            {
                completedAnimations++;
                _logger?.LogDebug("Завершена анимация {Completed}/{Total}", completedAnimations, totalAnimations);
                if (completedAnimations >= totalAnimations)
                {
                    _logger?.LogInformation("=== ВСЕ АНИМАЦИИ ПЕРВОГО РАСКРЫТИЯ ЗАВЕРШЕНЫ ===");
                    mainBorder.MinHeight = _config.ExpandedNotificationHeight;
                    _logger?.LogInformation("Установлена финальная высота: {MinHeight}", _config.ExpandedNotificationHeight);
                    System.Diagnostics.Debug.WriteLine("Все анимации расширения завершены");
                    System.Diagnostics.Debug.WriteLine($"Финальная ширина уведомления: {mainBorder.Width}, MinHeight: {mainBorder.MinHeight}");
                    
                    // Устанавливаем финальные значения для предотвращения проблем с отображением
                    contentPanel.Opacity = 1.0;
                    if (actionButton != null)
                    {
                        actionButton.Opacity = 1.0;
                        System.Diagnostics.Debug.WriteLine($"Финальное состояние кнопки: Visibility={actionButton.Visibility}, Opacity={actionButton.Opacity}");
                        System.Diagnostics.Debug.WriteLine($"Кнопка Grid.Column: {Grid.GetColumn(actionButton)}");
                        
                        if (actionButton is FrameworkElement fe)
                        {
                            System.Diagnostics.Debug.WriteLine($"Кнопка размеры: Width={fe.Width}, Height={fe.Height}, ActualWidth={fe.ActualWidth}, ActualHeight={fe.ActualHeight}");
                            System.Diagnostics.Debug.WriteLine($"Кнопка Margin: {fe.Margin}");
                            
                            if (fe is Control control)
                            {
                                System.Diagnostics.Debug.WriteLine($"Кнопка Padding: {control.Padding}");
                            }
                        }
                    }
                    
                    onCompleted?.Invoke();
                }
            };

            // Добавляем обработчики завершения
            widthAnimation.Completed += (s, e) => checkCompletion();
            heightAnimation.Completed += (s, e) => checkCompletion();
            contentOpacityAnimation.Completed += (s, e) => checkCompletion();
            
            if (leftAnimation != null)
            {
                leftAnimation.Completed += (s, e) => checkCompletion();
            }
            else
            {
                // Если анимация позиции не нужна, считаем её завершенной
                checkCompletion();
            }
            
            if (actionButtonOpacityAnimation != null)
            {
                actionButtonOpacityAnimation.Completed += (s, e) => checkCompletion();
            }

            // Запускаем все анимации одновременно
            _logger?.LogInformation("Запускаем все анимации первого раскрытия...");
            mainBorder.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
            _logger?.LogDebug("Запущена анимация ширины");
            
            mainBorder.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
            _logger?.LogDebug("Запущена анимация высоты");
            
            contentPanel.BeginAnimation(UIElement.OpacityProperty, contentOpacityAnimation);
            _logger?.LogDebug("Запущена анимация прозрачности контента");
            
            if (leftAnimation != null)
            {
                notification.BeginAnimation(Canvas.LeftProperty, leftAnimation);
                _logger?.LogDebug("Запущена анимация позиции");
            }
            
            if (actionButtonOpacityAnimation != null)
            {
                actionButton.BeginAnimation(UIElement.OpacityProperty, actionButtonOpacityAnimation);
                _logger?.LogDebug("Запущена анимация кнопки действия");
                System.Diagnostics.Debug.WriteLine($"Запускаем анимацию кнопки действия: Visibility={actionButton.Visibility}, Opacity={actionButton.Opacity}");
            }

            _logger?.LogInformation("=== ВСЕ АНИМАЦИИ ПЕРВОГО РАСКРЫТИЯ ЗАПУЩЕНЫ ===");
            _logger?.LogInformation("Ожидаем завершения через {Duration}ms", _config.ExpandAnimationDuration);
            System.Diagnostics.Debug.WriteLine($"Анимация расширения запущена. Duration: {_config.ExpandAnimationDuration}ms");
        }

        /// <summary>
        /// Анимация сжатия уведомления
        /// </summary>
        public void AnimateCompact(Border mainBorder, Panel contentPanel)
        {
            if (_disposed) return;
            
            if (!_config.EnableAnimations)
            {
                mainBorder.Width = _config.MinNotificationWidth;
                contentPanel.Opacity = 0.0;
                contentPanel.Visibility = Visibility.Collapsed;
                return;
            }

            // Создаем анимации
            var widthAnimation = CreateWidthAnimation(
                _config.MaxNotificationWidth, 
                _config.MinNotificationWidth, 
                300);

            var contentOpacityAnimation = CreateOpacityAnimation(1, 0, 200);

            // Добавляем обработчик для скрытия контента
            contentOpacityAnimation.Completed += (s, e) =>
            {
                contentPanel.Visibility = Visibility.Collapsed;
            };

            // Запускаем анимации
            mainBorder.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
            contentPanel.BeginAnimation(UIElement.OpacityProperty, contentOpacityAnimation);

            _logger?.LogDebug("Запущена анимация сжатия уведомления");
        }

        /// <summary>
        /// Анимация исчезновения уведомления
        /// </summary>
        public void AnimateDismiss(UserControl notification, Action onCompleted = null)
        {
            _logger?.LogInformation("=== НАЧАЛО АНИМАЦИИ ЗАКРЫТИЯ ===");
            _logger?.LogInformation("AnimateDismiss вызван. EnableAnimations: {EnableAnimations}", _config.EnableAnimations);
            
            if (_disposed) 
            {
                _logger?.LogWarning("Сервис анимации уже освобожден, пропускаем анимацию");
                onCompleted?.Invoke();
                return;
            }
            
            if (!_config.EnableAnimations)
            {
                _logger?.LogInformation("Анимации отключены, устанавливаем Opacity = 0.0");
                notification.Opacity = 0.0;
                onCompleted?.Invoke();
                return;
            }

            // Сохраняем текущую позицию для восстановления после анимации
            var originalLeft = Canvas.GetLeft(notification);
            var originalTop = Canvas.GetTop(notification);
            _logger?.LogInformation("Исходная позиция: Left={Left}, Top={Top}", originalLeft, originalTop);

            // Используем настройку длительности анимации закрытия
            var dismissDuration = _config.DismissAnimationDuration;
            _logger?.LogInformation("Параметры анимации: Duration={Duration}ms, SlideDistance={SlideDistance}px", 
                dismissDuration, _config.DismissSlideDistance);

            // Останавливаем все активные анимации для предотвращения конфликтов
            _logger?.LogDebug("Останавливаем все активные анимации");
            StopAllAnimations(notification);
            
            // Дополнительно останавливаем анимации позиционирования
            notification.BeginAnimation(Canvas.LeftProperty, null);
            notification.BeginAnimation(Canvas.TopProperty, null);
            System.Diagnostics.Debug.WriteLine("AnimateDismiss: Остановлены анимации позиционирования");
            
            // Явно фиксируем горизонтальную позицию для предотвращения смещения
            Canvas.SetLeft(notification, originalLeft);
            System.Diagnostics.Debug.WriteLine($"AnimateDismiss: Зафиксирована позиция Left={originalLeft}");
            _logger?.LogDebug("Остановлены анимации позиционирования, зафиксирована позиция Left={Left}", originalLeft);
            
            // Создаем анимацию сдвига вверх с прозрачностью
            var opacityAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(dismissDuration))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.Stop
            };
            _logger?.LogDebug("Создана анимация прозрачности: от 1.0 до 0.0 за {Duration}ms", dismissDuration);

            // Анимация сдвига вверх
            var currentTop = Canvas.GetTop(notification);
            var targetTop = currentTop - _config.DismissSlideDistance;
            var topAnimation = new DoubleAnimation(currentTop, targetTop, TimeSpan.FromMilliseconds(dismissDuration))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.Stop
            };
            _logger?.LogInformation("Создана анимация сдвига: от {CurrentTop} до {TargetTop} за {Duration}ms", 
                currentTop, targetTop, dismissDuration);

            // Добавляем обработчик завершения
            if (onCompleted != null)
            {
                opacityAnimation.Completed += (s, e) => 
                {
                    _logger?.LogInformation("=== АНИМАЦИЯ ЗАКРЫТИЯ ЗАВЕРШЕНА ===");
                    _logger?.LogInformation("Восстанавливаем исходную позицию: Left={Left}, Top={Top}", originalLeft, originalTop);
                    // Восстанавливаем исходную позицию
                    Canvas.SetLeft(notification, originalLeft);
                    Canvas.SetTop(notification, originalTop);
                    _logger?.LogInformation("Позиция восстановлена, вызываем callback");
                    onCompleted();
                };
            }

            try
            {
                _logger?.LogInformation("Запускаем анимации закрытия...");
                // Запускаем анимации сдвига вверх и прозрачности
                notification.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                _logger?.LogDebug("Запущена анимация прозрачности");
                
                notification.BeginAnimation(Canvas.TopProperty, topAnimation);
                _logger?.LogDebug("Запущена анимация сдвига");

                _logger?.LogInformation("=== АНИМАЦИИ ЗАПУЩЕНЫ УСПЕШНО ===");
                _logger?.LogInformation("Ожидаем завершения анимации через {Duration}ms", dismissDuration);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "=== ОШИБКА ЗАПУСКА АНИМАЦИИ ЗАКРЫТИЯ ===");
                _logger?.LogError("Используем graceful degradation - мгновенное закрытие");
                // Graceful degradation - мгновенное закрытие
                notification.Opacity = 0.0;
                Canvas.SetLeft(notification, originalLeft);
                Canvas.SetTop(notification, originalTop);
                _logger?.LogInformation("Graceful degradation выполнен, вызываем callback");
                onCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Останавливает все активные анимации для Border
        /// </summary>
        private void StopAllAnimations(Border border)
        {
            if (border == null) return;
            
            try
            {
                // Останавливаем все анимации Border
                border.BeginAnimation(FrameworkElement.WidthProperty, null);
                border.BeginAnimation(FrameworkElement.HeightProperty, null);
                border.BeginAnimation(FrameworkElement.MinHeightProperty, null);
                border.BeginAnimation(FrameworkElement.MaxHeightProperty, null);
                
                // Останавливаем анимации для всех дочерних элементов
                StopAnimationsRecursive(border);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Ошибка при остановке анимаций Border");
            }
        }

        /// <summary>
        /// Останавливает все активные анимации
        /// </summary>
        private void StopAllAnimations(UserControl notification)
        {
            if (notification == null) return;
            
            try
            {
                // Останавливаем все анимации уведомления
                notification.BeginAnimation(UIElement.OpacityProperty, null);
                notification.BeginAnimation(FrameworkElement.WidthProperty, null);
                notification.BeginAnimation(FrameworkElement.HeightProperty, null);
                notification.BeginAnimation(FrameworkElement.MinHeightProperty, null);
                notification.BeginAnimation(FrameworkElement.MaxHeightProperty, null);
                
                // Останавливаем анимации трансформации
                if (notification.RenderTransform is ScaleTransform existingTransform)
                {
                    existingTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                    existingTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                }
                
                // Останавливаем анимации Canvas позиционирования
                notification.BeginAnimation(Canvas.LeftProperty, null);
                notification.BeginAnimation(Canvas.TopProperty, null);
                
                // Останавливаем анимации для всех дочерних элементов
                StopAnimationsRecursive(notification);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Ошибка при остановке анимаций");
            }
        }

        /// <summary>
        /// Рекурсивно останавливает анимации для всех дочерних элементов
        /// </summary>
        private void StopAnimationsRecursive(DependencyObject parent)
        {
            if (parent == null) return;
            
            try
            {
                var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is FrameworkElement element)
                    {
                        element.BeginAnimation(UIElement.OpacityProperty, null);
                        element.BeginAnimation(FrameworkElement.WidthProperty, null);
                        element.BeginAnimation(FrameworkElement.HeightProperty, null);
                    }
                    StopAnimationsRecursive(child);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Ошибка при рекурсивной остановке анимаций");
            }
        }

        /// <summary>
        /// Создает анимацию масштабирования
        /// </summary>
        private DoubleAnimation CreateScaleAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };
        }

        /// <summary>
        /// Создает анимацию прозрачности
        /// </summary>
        private DoubleAnimation CreateOpacityAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                FillBehavior = FillBehavior.Stop
            };
        }

        /// <summary>
        /// Создает анимацию ширины
        /// </summary>
        private DoubleAnimation CreateWidthAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        private DoubleAnimation CreateHeightAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        /// <summary>
        /// Анимирует полное раскрытие уведомления по высоте (второй этап)
        /// </summary>
        public void AnimateFullyExpand(Border mainBorder, StackPanel contentPanel, Action onComplete = null, Button actionButton = null, UserControl notification = null)
        {
            _logger?.LogInformation("=== НАЧАЛО АНИМАЦИИ ПОЛНОГО РАСКРЫТИЯ ===");
            _logger?.LogInformation("AnimateFullyExpand вызван. EnableAnimations: {EnableAnimations}", _config.EnableAnimations);
            
            if (_disposed || mainBorder == null) 
            {
                _logger?.LogWarning("Сервис освобожден или mainBorder null, пропускаем анимацию");
                onComplete?.Invoke();
                return;
            }

            if (!_config.EnableAnimations)
            {
                _logger?.LogInformation("Анимации отключены, устанавливаем размеры напрямую");
                // Устанавливаем размеры напрямую без анимации
                mainBorder.Width = _config.MaxNotificationWidth;
                var baseHeight = _config.FullyExpandedBaseHeight;
                var actionsHeight = 0.0;
                
                if (contentPanel != null)
                {
                    var actionsPanel = contentPanel.FindName("ActionsPanel") as StackPanel;
                    if (actionsPanel != null && actionsPanel.Children.Count > 0)
                    {
                        actionsHeight = _config.ActionsPanelHeight;
                    }
                }
                
                var targetHeight = Math.Max(baseHeight + actionsHeight, _config.FullyExpandedMinHeight);
                mainBorder.Height = targetHeight;
                mainBorder.MinHeight = targetHeight;
                
                if (actionButton != null)
                {
                    actionButton.Visibility = Visibility.Visible;
                    actionButton.Opacity = 1.0;
                }
                
                _logger?.LogInformation("Размеры установлены напрямую: Width={Width}, Height={Height}", 
                    _config.MaxNotificationWidth, targetHeight);
                onComplete?.Invoke();
                return;
            }

            try
            {
                _logger?.LogInformation("Начинаем анимацию полного раскрытия по высоте");
                System.Diagnostics.Debug.WriteLine("Начинаем анимацию полного раскрытия по высоте");

                // Показываем действия при полном раскрытии
                if (actionButton != null)
                {
                    actionButton.Visibility = Visibility.Visible;
                    _logger?.LogDebug("Кнопка действия показана");
                }

                // Получаем текущую высоту
                var currentHeight = mainBorder.ActualHeight;
                _logger?.LogInformation("Текущая высота: {CurrentHeight}px", currentHeight);
                System.Diagnostics.Debug.WriteLine($"Текущая высота для полного раскрытия: {currentHeight}");

                // Временно устанавливаем фиксированную высоту для анимации
                mainBorder.Height = currentHeight;
                mainBorder.ClearValue(FrameworkElement.MaxHeightProperty);
                
                // Устанавливаем максимальную ширину для полного раскрытия
                mainBorder.Width = _config.MaxNotificationWidth;
                _logger?.LogInformation("Установлена ширина: {Width}px", _config.MaxNotificationWidth);
                
                // Рассчитываем высоту с учетом действий
                var baseHeight = _config.FullyExpandedBaseHeight;
                var actionsHeight = 0.0;
                _logger?.LogInformation("Базовая высота: {BaseHeight}px", baseHeight);
                
                // Если есть действия, добавляем высоту для них
                if (contentPanel != null)
                {
                    var actionsPanel = contentPanel.FindName("ActionsPanel") as StackPanel;
                    if (actionsPanel != null && actionsPanel.Children.Count > 0)
                    {
                        actionsHeight = _config.ActionsPanelHeight;
                        _logger?.LogInformation("Найдена панель действий с {Count} кнопками, добавляем {Height}px", 
                            actionsPanel.Children.Count, actionsHeight);
                        System.Diagnostics.Debug.WriteLine($"Найдена панель действий с {actionsPanel.Children.Count} кнопками, добавляем {actionsHeight}px");
                    }
                }
                
                var targetHeight = baseHeight + actionsHeight;
                _logger?.LogInformation("Рассчитанная высота: базовая {BaseHeight}px + действия {ActionsHeight}px = {TargetHeight}px", 
                    baseHeight, actionsHeight, targetHeight);
                System.Diagnostics.Debug.WriteLine($"Рассчитанная высота: базовая {baseHeight}px + действия {actionsHeight}px = {targetHeight}px");
                
                // Используем фиксированную высоту для полного раскрытия
                // Минимальная высота нужна только для предотвращения слишком маленьких уведомлений
                var minHeight = Math.Max(_config.MinNotificationHeight, _config.FullyExpandedMinHeight);
                if (targetHeight < minHeight)
                {
                    targetHeight = minHeight;
                    _logger?.LogInformation("Высота увеличена до минимальной: {MinHeight}px", minHeight);
                }
                
                _logger?.LogInformation("Целевая высота для полного раскрытия: {TargetHeight}px", targetHeight);
                System.Diagnostics.Debug.WriteLine($"Целевая высота для полного раскрытия: {targetHeight}");

                // Устанавливаем текущую высоту обратно для анимации
                mainBorder.Height = currentHeight;

                // Создаем анимацию высоты
                var heightAnimation = CreateHeightAnimation(currentHeight, targetHeight, (int)_config.ExpandAnimationDuration);
                _logger?.LogInformation("Создана анимация высоты: от {CurrentHeight} до {TargetHeight} за {Duration}ms", 
                    currentHeight, targetHeight, _config.ExpandAnimationDuration);
                
                // Обработчик завершения анимации
                Action onHeightAnimationComplete = () =>
                {
                    _logger?.LogInformation("=== АНИМАЦИЯ ПОЛНОГО РАСКРЫТИЯ ЗАВЕРШЕНА ===");
                    _logger?.LogInformation("Финальная высота: {FinalHeight}px", mainBorder.ActualHeight);
                    System.Diagnostics.Debug.WriteLine($"Анимация полного раскрытия завершена. Финальная высота: {mainBorder.ActualHeight}");
                    
                    // После анимации устанавливаем рассчитанную высоту для полного раскрытия
                    mainBorder.Height = targetHeight;
                    mainBorder.MinHeight = targetHeight;
                    // Убеждаемся, что ширина остается максимальной
                    mainBorder.Width = _config.MaxNotificationWidth;
                    _logger?.LogInformation("Установлены финальные размеры: Width={Width}px, Height={Height}px", 
                        _config.MaxNotificationWidth, targetHeight);
                    
                    // Устанавливаем финальные значения для предотвращения проблем с отображением
                    if (contentPanel != null)
                    {
                        contentPanel.Opacity = 1.0;
                    }
                    if (actionButton != null)
                    {
                        actionButton.Opacity = 1.0;
                    }
                    
                    _logger?.LogInformation("Вызываем callback завершения");
                    onComplete?.Invoke();
                };
                
                heightAnimation.Completed += (s, e) => onHeightAnimationComplete();

                // Запускаем анимацию высоты
                _logger?.LogInformation("Запускаем анимацию высоты...");
                _logger?.LogInformation("Текущая ширина: {Width}px, MinHeight: {MinHeight}px", 
                    mainBorder.ActualWidth, mainBorder.MinHeight);
                mainBorder.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
                _logger?.LogInformation("=== АНИМАЦИЯ ПОЛНОГО РАСКРЫТИЯ ЗАПУЩЕНА ===");
                _logger?.LogInformation("Ожидаем завершения через {Duration}ms", _config.ExpandAnimationDuration);
                System.Diagnostics.Debug.WriteLine($"Запускаем анимацию высоты от {currentHeight} до {targetHeight}");
                System.Diagnostics.Debug.WriteLine($"Текущая ширина: {mainBorder.ActualWidth}, MinHeight: {mainBorder.MinHeight}");

                // Анимируем прозрачность действий
                if (actionButton != null)
                {
                    var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(_config.ExpandAnimationDuration))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    actionButton.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                }

                System.Diagnostics.Debug.WriteLine("Анимация полного раскрытия запущена");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при анимации полного раскрытия: {ex.Message}");
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _logger?.LogDebug("NotificationAnimationService освобожден");
            }
        }
    }
}
