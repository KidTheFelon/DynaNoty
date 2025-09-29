using System;

namespace DynaNoty.Configuration
{
    /// <summary>
    /// Настройки физических анимаций
    /// </summary>
    public class PhysicsAnimationSettings
    {
        /// <summary>
        /// Напряжение пружины для spring-анимаций (100-600)
        /// </summary>
        public double SpringTension { get; set; } = 300;

        /// <summary>
        /// Трение пружины для spring-анимаций (10-50)
        /// </summary>
        public double SpringFriction { get; set; } = 30;

        /// <summary>
        /// Количество колебаний для bounce-анимаций
        /// </summary>
        public int BounceCount { get; set; } = 3;

        /// <summary>
        /// Амплитуда отскока для bounce-анимаций (0.1-0.8)
        /// </summary>
        public double BounceAmplitude { get; set; } = 0.3;

        /// <summary>
        /// Количество колебаний для elastic-анимаций
        /// </summary>
        public int ElasticOscillations { get; set; } = 3;

        /// <summary>
        /// Упругость для elastic-анимаций (0.1-0.5)
        /// </summary>
        public double ElasticSpringiness { get; set; } = 0.3;

        /// <summary>
        /// Начальная скорость для momentum-анимаций
        /// </summary>
        public double MomentumVelocity { get; set; } = 1.0;

        /// <summary>
        /// Коэффициент трения для momentum-анимаций (0.8-0.99)
        /// </summary>
        public double MomentumFriction { get; set; } = 0.95;

        /// <summary>
        /// Использовать физическое репозиционирование (пружина вместо ease)
        /// </summary>
        public bool UsePhysicsForReposition { get; set; } = false;

        /// <summary>
        /// Пресет настроек физики (для быстрого тюнинга)
        /// </summary>
        public PhysicsPreset Preset { get; set; } = PhysicsPreset.Natural;

        /// <summary>
        /// Использовать физические анимации для появления
        /// </summary>
        public bool UsePhysicsForAppear { get; set; } = false;

        /// <summary>
        /// Использовать физические анимации для расширения
        /// </summary>
        public bool UsePhysicsForExpand { get; set; } = false;

        /// <summary>
        /// Использовать физические анимации для исчезновения
        /// </summary>
        public bool UsePhysicsForDismiss { get; set; } = false;

        /// <summary>
        /// Тип физической анимации
        /// </summary>
        public PhysicsAnimationType AnimationType { get; set; } = PhysicsAnimationType.Spring;
    }

    /// <summary>
    /// Типы физических анимаций
    /// </summary>
    public enum PhysicsAnimationType
    {
        /// <summary>
        /// Пружинная анимация
        /// </summary>
        Spring,

        /// <summary>
        /// Анимация с отскоком
        /// </summary>
        Bounce,

        /// <summary>
        /// Упругая анимация
        /// </summary>
        Elastic,

        /// <summary>
        /// Анимация с инерцией
        /// </summary>
        Momentum
    }

    /// <summary>
    /// Предустановленные наборы параметров физики
    /// </summary>
    public enum PhysicsPreset
    {
        /// <summary>
        /// Естественный, сбалансированный
        /// </summary>
        Natural,

        /// <summary>
        /// Быстрый и резкий отклик
        /// </summary>
        Snappy,

        /// <summary>
        /// Игривый с большей упругостью
        /// </summary>
        Playful
    }
}
