using System;
using DynaNoty.Configuration;
using DynaNoty.Services;

namespace DynaNoty.Examples
{
    /// <summary>
    /// Пример использования физических анимаций
    /// </summary>
    public class PhysicsAnimationExample
    {
        public static void DemonstratePhysicsAnimations()
        {
            // Создаем конфигурацию с физическими анимациями
            var config = new NotificationConfiguration
            {
                PhysicsAnimations = new PhysicsAnimationSettings
                {
                    // Используем spring-анимации для появления
                    AnimationType = PhysicsAnimationType.Spring,
                    UsePhysicsForAppear = true,
                    UsePhysicsForExpand = false,
                    UsePhysicsForDismiss = true,
                    
                    // Настройки пружины
                    SpringTension = 400,  // Напряжение пружины
                    SpringFriction = 40   // Трение пружины
                }
            };

            // Альтернативно: bounce-анимации
            var bounceConfig = new NotificationConfiguration
            {
                PhysicsAnimations = new PhysicsAnimationSettings
                {
                    AnimationType = PhysicsAnimationType.Bounce,
                    UsePhysicsForAppear = true,
                    BounceCount = 3,
                    BounceAmplitude = 0.4
                }
            };

            // Elastic-анимации с колебаниями
            var elasticConfig = new NotificationConfiguration
            {
                PhysicsAnimations = new PhysicsAnimationSettings
                {
                    AnimationType = PhysicsAnimationType.Elastic,
                    UsePhysicsForAppear = true,
                    ElasticOscillations = 2,
                    ElasticSpringiness = 0.4
                }
            };

            // Momentum-анимации с инерцией
            var momentumConfig = new NotificationConfiguration
            {
                PhysicsAnimations = new PhysicsAnimationSettings
                {
                    AnimationType = PhysicsAnimationType.Momentum,
                    UsePhysicsForAppear = true,
                    MomentumVelocity = 1.2,
                    MomentumFriction = 0.92
                }
            };

            // Создаем сервисы с физическими анимациями
            var animationFactory = new AnimationFactory(config);
            var animationService = new NotificationAnimationService(config);

            Console.WriteLine("Физические анимации настроены:");
            Console.WriteLine($"- Тип: {config.PhysicsAnimations.AnimationType}");
            Console.WriteLine($"- Spring Tension: {config.PhysicsAnimations.SpringTension}");
            Console.WriteLine($"- Spring Friction: {config.PhysicsAnimations.SpringFriction}");
        }
    }
}
