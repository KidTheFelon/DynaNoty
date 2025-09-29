using System.Collections.Generic;
using DynaNoty.Models;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Реестр обработчиков типов уведомлений
    /// </summary>
    public interface INotificationTypeHandlerRegistry
    {
        /// <summary>
        /// Получает обработчик для указанного типа уведомления
        /// </summary>
        INotificationTypeHandler GetHandler(NotificationType type);

        /// <summary>
        /// Регистрирует обработчик для типа уведомления
        /// </summary>
        void RegisterHandler(INotificationTypeHandler handler);

        /// <summary>
        /// Получает все зарегистрированные обработчики
        /// </summary>
        IEnumerable<INotificationTypeHandler> GetAllHandlers();
    }
}
