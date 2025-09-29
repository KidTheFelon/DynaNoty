using System;
using System.Windows.Controls;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для окна уведомлений
    /// </summary>
    public interface INotificationWindow : IDisposable
    {
        /// <summary>
        /// Контейнер для размещения уведомлений
        /// </summary>
        Panel Container { get; }

        /// <summary>
        /// Показать окно уведомлений
        /// </summary>
        void Show();

        /// <summary>
        /// Скрыть окно уведомлений
        /// </summary>
        void Hide();
    }
}
