using System.Collections.Generic;
using System.Windows;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для позиционирования уведомлений
    /// </summary>
    public interface INotificationPositioningService
    {
        /// <summary>
        /// Позиционирует уведомление на экране
        /// </summary>
        void PositionNotification(UIElement notification);

        /// <summary>
        /// Вычисляет позицию для множественных уведомлений
        /// </summary>
        void PositionNotification(UIElement notification, int index);

        /// <summary>
        /// Анимированно перемещает уведомление в новую позицию
        /// </summary>
        void AnimateToPosition(UIElement notification, int index);

        /// <summary>
        /// Пересчитывает позиции всех уведомлений с учетом их реальной высоты
        /// </summary>
        void RecalculateAllPositions(IEnumerable<UIElement> notifications);

        /// <summary>
        /// Пересчитывает позицию уведомления после изменения размера
        /// </summary>
        void RecalculatePositionAfterResize(UIElement notification, int index);
    }
}
