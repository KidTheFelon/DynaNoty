# Архитектура DynaNoty

## Обзор

DynaNoty - это WPF приложение для создания Dynamic Island-стиля уведомлений на Windows с современной архитектурой, основанной на принципах SOLID и лучших практиках разработки.

## Архитектурные принципы

### SOLID принципы

1. **Single Responsibility Principle (SRP)**
   - `NotificationManager` - управление уведомлениями
   - `NotificationWindow` - отображение окна уведомлений
   - `DynamicIslandNotification` - UI компонент уведомления
   - `NotificationData` - модель данных

2. **Open/Closed Principle (OCP)**
   - Интерфейсы позволяют расширять функциональность без изменения существующего кода
   - Новые типы уведомлений добавляются через enum и factory methods

3. **Liskov Substitution Principle (LSP)**
   - Все реализации интерфейсов полностью заменяют базовые интерфейсы
   - `NotificationWindow` может быть заменен mock'ом в тестах

4. **Interface Segregation Principle (ISP)**
   - `INotificationService` - только методы работы с уведомлениями
   - `INotificationWindow` - только методы управления окном

5. **Dependency Inversion Principle (DIP)**
   - Зависимости инвертированы через интерфейсы
   - Dependency Injection контейнер управляет зависимостями

## Структура проекта

```
DynaNoty/
├── Interfaces/              # 20+ интерфейсов
│   ├── INotificationService.cs
│   ├── INotificationWindow.cs
│   ├── IAnimationCache.cs
│   ├── IPerformanceMonitor.cs
│   └── ... (другие интерфейсы)
├── Services/               # 30+ сервисов
│   ├── NotificationManager.cs
│   ├── NotificationWindow.cs
│   ├── Handlers/           # Обработчики уведомлений
│   │   ├── StandardNotificationHandler.cs
│   │   ├── MusicNotificationHandler.cs
│   │   ├── CallNotificationHandler.cs
│   │   └── CompactNotificationHandler.cs
│   └── ... (другие сервисы)
├── Models/                # Модели данных
│   ├── NotificationData.cs
│   └── NotificationAction.cs
├── Events/                # Аргументы событий
│   └── NotificationDismissedEventArgs.cs
├── Configuration/         # 6 классов конфигурации
│   ├── NotificationConfiguration.cs
│   ├── NotificationAppearanceConfig.cs
│   ├── NotificationBehaviorConfig.cs
│   └── ... (другие конфигурации)
├── ViewModels/            # ViewModels
│   └── SettingsViewModel.cs
├── Constants/             # Константы
│   └── NotificationConstants.cs
├── Tests/                 # Тесты
├── DynamicIslandNotification.xaml/cs # UI компонент
├── MainWindow.xaml/cs     # Главное окно
├── SettingsWindow.xaml/cs # Окно настроек
└── App.xaml/cs           # Точка входа
```

## Компоненты

### INotificationService
Интерфейс сервиса уведомлений с методами:
- `ShowNotification()` - обычные уведомления
- `ShowMusicNotification()` - музыкальные уведомления
- `ShowCallNotification()` - уведомления о звонках
- `ShowCompactNotification()` - компактные уведомления
- `ClearAllNotifications()` - очистка всех уведомлений

### NotificationManager
Реализация сервиса уведомлений:
- Управляет жизненным циклом уведомлений
- Позиционирует уведомления на экране
- Обрабатывает события закрытия и действий
- Реализует IDisposable для правильного освобождения ресурсов

### NotificationWindow
Обертка для окна уведомлений:
- Создает невидимое окно для уведомлений
- Предоставляет контейнер для размещения уведомлений
- Управляет видимостью окна

### NotificationData
Модель данных уведомления:
- Содержит все необходимые свойства
- Factory methods для создания разных типов уведомлений
- Автоматическая генерация ID и timestamp

## Dependency Injection

### Конфигурация сервисов
```csharp
services.AddDynaNotyServices()
```

Регистрирует:
- `INotificationService` → `NotificationManager`
- `INotificationWindow` → `NotificationWindow`
- `ILogger` → Debug logger

### Использование в приложении
```csharp
var notificationService = serviceProvider.GetRequiredService<INotificationService>();
```

## Управление ресурсами

### IDisposable Pattern
- `NotificationManager` реализует IDisposable
- `NotificationWindow` реализует IDisposable
- Правильное освобождение DispatcherTimer и Window
- Проверка disposed состояния в методах

### Освобождение ресурсов
```csharp
using var notificationService = new NotificationManager();
// Автоматическое освобождение при выходе из области видимости
```

## Обработка ошибок

### Структурированное логирование
- Использование ILogger для всех операций
- Разные уровни логирования (Debug, Information, Error)
- Контекстная информация в логах

### Try-Catch блоки
- Обработка исключений в критических местах
- Логирование ошибок с контекстом
- Graceful degradation при ошибках

## Тестирование

### Простые тесты
- Без внешних зависимостей
- Тестирование основных сценариев
- Валидация создания моделей данных
- Проверка корректности работы сервисов

### Тестовое покрытие
- NotificationData - создание и валидация
- NotificationWindow - базовая функциональность
- NotificationManager - основные операции

## Конфигурация

### NotificationConfiguration
Настройки системы уведомлений:
- Время автоматического скрытия
- Размеры уведомлений
- Отступы и позиционирование
- Уровни логирования

## События

### Типизированные аргументы
- `NotificationDismissedEventArgs` - данные о закрытии
- `NotificationActionEventArgs` - данные о действиях
- Timestamp и ID уведомления в каждом событии

### Подписка на события
```csharp
notificationService.NotificationDismissed += (sender, e) => {
    Console.WriteLine($"Уведомление {e.NotificationId} закрыто в {e.DismissedAt}");
};
```

## Преимущества архитектуры

1. **Тестируемость** - интерфейсы позволяют легко создавать моки
2. **Расширяемость** - легко добавлять новые типы уведомлений
3. **Поддерживаемость** - четкое разделение ответственностей
4. **Надежность** - правильное управление ресурсами и обработка ошибок
5. **Производительность** - эффективное использование ресурсов

## Будущие улучшения

### Основная цель
1. **Интеграция с системными уведомлениями Windows 10/11** - главная задача проекта
   - Перехват системных уведомлений Windows
   - Отображение их в стиле Dynamic Island
   - Настройка фильтрации уведомлений
   - Интеграция с Action Center

### Дополнительные улучшения
2. **Настраиваемые анимации** - пользовательские анимации
3. **Группировка уведомлений** - объединение похожих уведомлений
4. **Поддержка звуков** - звуковые эффекты для уведомлений
5. **Улучшения производительности** - оптимизация работы
6. **Plugin System** - система плагинов для расширений
