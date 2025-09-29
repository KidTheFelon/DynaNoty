using DynaNoty.Interfaces;
using DynaNoty.Configuration;
using Microsoft.Extensions.Logging;

namespace DynaNoty
{
    /// <summary>
    /// Контейнер зависимостей для NotificationManager
    /// </summary>
    public class NotificationManagerDependencies
    {
        public INotificationWindow NotificationWindow { get; set; }
        public NotificationConfiguration Config { get; set; }
        public INotificationQueue Queue { get; set; }
        public INotificationDisplayManager DisplayManager { get; set; }
        public INotificationLifecycleManager LifecycleManager { get; set; }
        public IErrorHandler ErrorHandler { get; set; }
        public IRateLimiter RateLimiter { get; set; }
        public IPerformanceMonitor PerformanceMonitor { get; set; }
        public ISystemNotificationService SystemNotificationService { get; set; }
        public ILogger<NotificationManager> Logger { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
    }
}
