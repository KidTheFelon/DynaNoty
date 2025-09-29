using System;
using System.Windows;
using System.Windows.Media;

namespace DynaNoty.Interfaces
{
    /// <summary>
    /// Интерфейс для оптимизированного управления анимациями
    /// </summary>
    public interface IAnimationOptimizer : IDisposable
    {
        /// <summary>
        /// Плавная анимация свойства с использованием CompositionTarget
        /// </summary>
        void AnimateProperty<T>(DependencyObject target, DependencyProperty property,
            T fromValue, T toValue, TimeSpan duration, Action<T> onUpdate, Action onCompleted = null);
    }
}
