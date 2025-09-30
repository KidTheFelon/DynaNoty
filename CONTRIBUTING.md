# 🤝 Руководство по внесению вклада в DynaNoty

Спасибо за интерес к проекту DynaNoty! 

**Этот проект создан полностью с помощью нейросетей** и мы приветствуем как AI-разработчиков, так и традиционных разработчиков!

## 🤝 Сообщество проекта

Наше сообщество состоит из:
- **AI-разработчиков** - используют нейросети для написания кода
- **Традиционных разработчиков** - пишут код вручную
- **Гибридных разработчиков** - сочетают оба подхода

**Важно**: Мы приветствуем оба подхода и просим относиться друг к другу с уважением. Цель проекта - прогресс, а не консервативность или излишний либерализм. Каждый вклад ценен!

## 📋 Содержание

- [Код поведения](#код-поведения)
- [Как внести вклад](#как-внести-вклад)
- [Процесс разработки](#процесс-разработки)
- [Стандарты кода](#стандарты-кода)
- [Тестирование](#тестирование)
- [Документация](#документация)
- [Вопросы и поддержка](#вопросы-и-поддержка)

## Код поведения

Этот проект следует [Contributor Covenant](https://www.contributor-covenant.org/). Участвуя в проекте, вы соглашаетесь соблюдать его условия.

### Принципы сообщества

- **Толерантность**: Уважайте разные подходы к разработке (AI vs традиционный)
- **Прогресс**: Фокус на улучшении проекта, а не на спорах о методах
- **Конструктивность**: Критика должна быть обоснованной и полезной
- **Открытость**: Приветствуем новые идеи и подходы
- **Взаимопомощь**: Помогайте друг другу, независимо от используемых инструментов

## Как внести вклад

### Сообщение об ошибках

1. **Проверьте существующие issues** - возможно, проблема уже известна
2. **Используйте шаблон** - заполните все поля в issue template
3. **Опишите подробно**:
   - Шаги для воспроизведения
   - Ожидаемое поведение
   - Фактическое поведение
   - Скриншоты/видео
   - Логи и стектрейсы
   - Версия .NET, Windows, и т.д.

### Предложения улучшений

1. **Создайте issue** с тегом `enhancement`
2. **Опишите проблему** - что не работает или что можно улучшить
3. **Предложите решение** - как вы видите реализацию
4. **Обсудите** - ждите обратной связи от мейнтейнеров

### Внесение кода

**Выберите свой подход к разработке!**

1. **Форкните репозиторий**
2. **Создайте feature branch от develop**:
   ```bash
   git checkout develop
   git checkout -b feature/amazing-feature
   # или
   git checkout -b fix/bug-description
   ```
3. **Выберите метод разработки**:
   - **AI-разработка**: Используйте ChatGPT, GitHub Copilot, Claude и т.д.
   - **Традиционная разработка**: Пишите код вручную
   - **Гибридный подход**: Сочетайте оба метода
4. **Проверьте форматирование**:
   ```bash
   dotnet format --verify-no-changes
   ```
5. **Добавьте тесты** (любым способом)
6. **Обновите документацию** при необходимости
7. **Сделайте коммит**:
   ```bash
   git commit -m "feat: add amazing feature (AI-generated)"
   # или
   git commit -m "feat: add amazing feature (hand-written)"
   # или
   git commit -m "feat: add amazing feature (AI-assisted)"
   ```
8. **Запушьте изменения**:
   ```bash
   git push origin feature/amazing-feature
   ```
9. **Создайте Pull Request** `feature/amazing-feature` → `develop` с указанием метода разработки

## Процесс разработки

### Настройка окружения

1. **Клонируйте форк**:
   ```bash
   git clone https://github.com/yourusername/DynaNoty.git
   cd DynaNoty
   ```

2. **Установите зависимости**:
   ```bash
   dotnet restore
   ```

3. **Соберите проект**:
   ```bash
   dotnet build
   ```

4. **Запустите приложение**:
   ```bash
   dotnet run
   ```

### Workflow

**Структура веток:**
- `main` - стабильная версия, только релизы
- `develop` - активная разработка, интеграция фич

1. **Синхронизируйтесь с upstream**:
   ```bash
   git remote add upstream https://github.com/original/DynaNoty.git
   git fetch upstream
   git checkout develop
   git merge upstream/develop
   ```

2. **Создайте feature ветку от develop**:
   ```bash
   git checkout develop
   git checkout -b feature/your-feature-name
   ```

3. **Внесите изменения** с частыми коммитами
4. **Проверьте сборку** перед каждым коммитом: `dotnet build`
5. **Обновите документацию** при необходимости
6. **Создайте PR**: `feature/your-feature` → `develop`
7. **После мержа в develop** - создайте PR: `develop` → `main` для релиза

## Важные ограничения

**Это оригинальная работа!**

- ❌ **НЕ копируйте** код в свои проекты
- ❌ **НЕ используйте** в коммерческих целях
- ❌ **НЕ создавайте** производные работы
- ❌ **НЕ создавайте** форки для полного копирования
- ✅ **Вносите вклад** в развитие этого проекта
- ✅ **Изучайте** архитектуру и подходы
- ✅ **Вдохновляйтесь** идеями

## Стандарты кода

### C# Style Guide

```csharp
// ✅ Хорошо
public class NotificationManager : INotificationService, IDisposable
{
    private readonly ILogger<NotificationManager> _logger;
    private readonly INotificationWindow _window;
    private bool _disposed;

    public NotificationManager(
        ILogger<NotificationManager> logger,
        INotificationWindow window)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public void ShowNotification(string title, string subtitle, string icon)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(NotificationManager));

        _logger.LogInformation("Showing notification: {Title}", title);
        
        try
        {
            // Реализация
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show notification: {Title}", title);
            throw;
        }
    }
}
```

### Правила именования

- **Классы**: `PascalCase` (NotificationManager)
- **Методы**: `PascalCase` (ShowNotification)
- **Свойства**: `PascalCase` (NotificationId)
- **Поля**: `_camelCase` (_logger, _disposed)
- **Константы**: `UPPER_CASE` (MAX_NOTIFICATION_WIDTH)
- **Интерфейсы**: `IPascalCase` (INotificationService)

### Структура файлов

```
Services/
├── NotificationManager.cs
├── NotificationWindow.cs
└── Handlers/
    ├── StandardNotificationHandler.cs
    └── MusicNotificationHandler.cs
```

### Комментарии

```csharp
/// <summary>
/// Показывает обычное уведомление с заголовком, подзаголовком и иконкой.
/// </summary>
/// <param name="title">Заголовок уведомления</param>
/// <param name="subtitle">Подзаголовок уведомления</param>
/// <param name="icon">Иконка уведомления (эмодзи или текст)</param>
/// <exception cref="ArgumentNullException">Когда title равен null</exception>
/// <exception cref="ObjectDisposedException">Когда объект уже освобожден</exception>
public void ShowNotification(string title, string subtitle, string icon)
{
    // Реализация
}
```

## Тестирование

### Текущее состояние

**Примечание**: В проекте пока нет автоматических тестов. Папка `Tests/` пустая.

### Планируемое тестирование

При добавлении тестов рекомендуется:

- **Unit тесты** для всех публичных методов
- **Integration тесты** для взаимодействия компонентов
- **Edge cases** - граничные случаи
- **Error handling** - обработка ошибок

### Ручное тестирование

Пока тесты не добавлены, используйте:

1. **Запуск приложения**: `dotnet run`
2. **Тестирование UI**: нажимайте кнопки в главном окне
3. **Проверка логов**: следите за выводами в консоли
4. **Проверка сборки**: `dotnet build` перед коммитами

## Форматирование кода

### Автоматическое форматирование

Проект использует `dotnet format` для проверки стиля кода:

```bash
# Проверить форматирование
dotnet format --verify-no-changes

# Исправить форматирование
dotnet format
```

### CI/CD проверки

GitHub Actions автоматически проверяет форматирование при каждом PR. Если проверка не проходит, исправьте форматирование и запушьте изменения.

## Документация

### Обновление документации

- **README.md** - при добавлении новых функций
- **ARCHITECTURE.md** - при изменении архитектуры
- **API документация** - для новых публичных методов
- **Примеры** - в коде проекта (MainWindow.xaml.cs, NotificationManager.cs, Services/Handlers/)

### Документирование API

```csharp
/// <summary>
/// Сервис для управления уведомлениями в стиле Dynamic Island.
/// </summary>
/// <remarks>
/// Этот сервис предоставляет методы для отображения различных типов уведомлений
/// с поддержкой анимаций, автоматического скрытия и интерактивности.
/// </remarks>
/// <example>
/// <code>
/// var service = serviceProvider.GetRequiredService&lt;INotificationService&gt;();
/// service.ShowNotification("Заголовок", "Подзаголовок", "🔔");
/// </code>
/// </example>
public interface INotificationService
{
    // Методы
}
```

## Типы коммитов

Используйте [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` - новая функциональность
- `fix:` - исправление бага
- `docs:` - изменения в документации
- `style:` - форматирование, отсутствующие точки с запятой и т.д.
- `refactor:` - рефакторинг кода
- `test:` - добавление тестов
- `chore:` - обновление задач сборки, конфигурации и т.д.

### Примеры

```bash
git commit -m "feat: add support for custom animations"
git commit -m "fix: resolve memory leak in notification window"
git commit -m "docs: update API documentation for new methods"
git commit -m "test: add unit tests for notification handlers"
```

## Code Review

### Что проверяется

- **Функциональность** - код работает как ожидается
- **Тестирование** - есть соответствующие тесты
- **Производительность** - нет очевидных проблем с производительностью
- **Безопасность** - нет уязвимостей
- **Читаемость** - код понятен и хорошо структурирован
- **Документация** - обновлена при необходимости

### Процесс

1. **Автоматические проверки** - CI/CD pipeline проверяет:
   - Сборку проекта (`dotnet build`)
   - Форматирование кода (`dotnet format --verify-no-changes`)
   - Тесты (`dotnet test`) - пока не реализованы
2. **Ручной review** - минимум 1 одобрение
3. **Тестирование** - ручное тестирование функциональности
4. **Слияние** - только после одобрения

### Защита веток

- **main** - защищена, только через PR из develop
- **develop** - основная ветка для разработки, все feature ветки мержатся сюда
- **feature ветки** - создаются от develop, мержатся обратно в develop

## ❓ Вопросы и поддержка

### Где получить помощь

- **GitHub Issues** - для багов и предложений
- **GitHub Discussions** - для вопросов и обсуждений


### Часто задаваемые вопросы

**Q: Как добавить новый тип уведомления?**
A: Создайте новый handler в папке Services/Handlers/ и зарегистрируйте его в NotificationTypeHandlerRegistry.

**Q: Как настроить анимации?**
A: Используйте NotificationConfiguration для настройки длительности и типа анимаций.

**Q: Как добавить поддержку звуков?**
A: Создайте новый сервис ISoundService и интегрируйте его в NotificationManager.

## 🎯 Приоритетные задачи

### Критический приоритет
- [ ] **Интеграция с системными уведомлениями Windows 10/11** - главная цель проекта
  - Перехват системных уведомлений Windows
  - Отображение их в стиле Dynamic Island
  - Настройка фильтрации уведомлений
  - Интеграция с Action Center

### Высокий приоритет
- [ ] Улучшение производительности анимаций
- [ ] Добавление автоматических тестов
- [ ] Поддержка группировки уведомлений

### Средний приоритет
- [ ] Настраиваемые звуки
- [ ] Плагин система
- [ ] Настраиваемые анимации

### Низкий приоритет
- [ ] 3D анимации
- [ ] Расширенные эффекты
- [ ] Дополнительные темы

## 🏆 Признание контрибьюторов

Все контрибьюторы будут упомянуты в:
- README.md (список контрибьюторов)
- CHANGELOG.md (для значительных вкладов)
- Release notes

---

**Спасибо за ваш вклад в DynaNoty! 🎉**
