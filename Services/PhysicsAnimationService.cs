using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Extensions.Logging;

namespace DynaNoty.Services
{
    /// <summary>
    /// Сервис для физически корректных анимаций в стиле iOS
    /// </summary>
    public class PhysicsAnimationService
    {
        private readonly ILogger<PhysicsAnimationService> _logger;

        public PhysicsAnimationService(ILogger<PhysicsAnimationService> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Создает spring-анимацию с физическими параметрами
        /// </summary>
        public DoubleAnimation CreateSpringAnimation(double from, double to, double springTension = 300, double springFriction = 30)
        {
            var animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new SpringEasingFunction
                {
                    SpringTension = springTension,
                    SpringFriction = springFriction,
                    // Если OscillationCount == 0, будет критическое демпфирование (см. EaseInCore)
                    OscillationCount = 0
                },
                FillBehavior = FillBehavior.Stop
            };

            return animation;
        }

        /// <summary>
        /// Создает bounce-анимацию с упругостью
        /// </summary>
        public DoubleAnimation CreateBounceAnimation(double from, double to, double bounceAmplitude = 0.3)
        {
            var animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new BounceEase
                {
                    Bounces = 3,
                    Bounciness = bounceAmplitude,
                    EasingMode = EasingMode.EaseOut
                },
                FillBehavior = FillBehavior.Stop
            };

            return animation;
        }

        /// <summary>
        /// Создает momentum-анимацию с инерцией
        /// </summary>
        public DoubleAnimation CreateMomentumAnimation(double from, double to, double velocity, double friction = 0.95)
        {
            var distance = Math.Abs(to - from);
            var duration = CalculateMomentumDuration(distance, velocity, friction);

            var animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration))
            {
                EasingFunction = new MomentumEasingFunction
                {
                    InitialVelocity = velocity,
                    Friction = friction
                },
                FillBehavior = FillBehavior.Stop
            };

            return animation;
        }

        /// <summary>
        /// Создает elastic-анимацию с затухающими колебаниями
        /// </summary>
        public DoubleAnimation CreateElasticAnimation(double from, double to, int oscillationCount = 3, double springiness = 0.3)
        {
            var animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(800))
            {
                EasingFunction = new ElasticEase
                {
                    Oscillations = oscillationCount,
                    Springiness = springiness,
                    EasingMode = EasingMode.EaseOut
                },
                FillBehavior = FillBehavior.Stop
            };

            return animation;
        }

        private int CalculateMomentumDuration(double distance, double velocity, double friction)
        {
            // Упрощенный расчет на основе физики
            var time = Math.Log(distance / velocity) / Math.Log(friction);
            return Math.Max(100, Math.Min(1000, (int)(time * 1000)));
        }
    }

    /// <summary>
    /// Кастомная easing-функция для spring-анимаций
    /// </summary>
    public class SpringEasingFunction : EasingFunctionBase
    {
        public double SpringTension { get; set; } = 300;
        public double SpringFriction { get; set; } = 30;
        public int OscillationCount { get; set; } = 0;

        protected override double EaseInCore(double normalizedTime)
        {
            var tension = SpringTension / 1000.0;
            var friction = SpringFriction / 1000.0;

            // Критическое демпфирование при нулевых осцилляциях
            if (OscillationCount <= 0)
            {
                // Модель exp затухания без колебаний
                var damping = Math.Exp(-friction * normalizedTime * 12);
                return 1.0 - damping * (1.0 - normalizedTime);
            }

            // Упрощенная spring-формула с осцилляциями
            var dampingOsc = Math.Exp(-friction * normalizedTime * 10);
            var oscillation = Math.Cos(tension * normalizedTime * Math.PI * 2 * Math.Max(1, OscillationCount));
            return 1.0 - (dampingOsc * (1.0 - normalizedTime) * (1.0 + oscillation * 0.1));
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new SpringEasingFunction();
        }
    }

    /// <summary>
    /// Кастомная easing-функция для momentum-анимаций
    /// </summary>
    public class MomentumEasingFunction : EasingFunctionBase
    {
        public double InitialVelocity { get; set; } = 1.0;
        public double Friction { get; set; } = 0.95;

        protected override double EaseInCore(double normalizedTime)
        {
            // Имитация инерции с затуханием
            var velocity = InitialVelocity * Math.Pow(Friction, normalizedTime * 10);
            return Math.Min(1.0, normalizedTime * velocity);
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new MomentumEasingFunction();
        }
    }
}
