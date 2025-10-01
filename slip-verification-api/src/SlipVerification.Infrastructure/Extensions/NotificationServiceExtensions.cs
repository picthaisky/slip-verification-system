using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlipVerification.Application.Interfaces.Notifications;
using SlipVerification.Infrastructure.Services.Notifications;

namespace SlipVerification.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring notification services
/// </summary>
public static class NotificationServiceExtensions
{
    /// <summary>
    /// Adds notification services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options from appsettings
        services.Configure<LineNotifyOptions>(
            configuration.GetSection(LineNotifyOptions.SectionName));
        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));
        services.Configure<PushNotificationOptions>(
            configuration.GetSection(PushNotificationOptions.SectionName));
        services.Configure<SmsOptions>(
            configuration.GetSection(SmsOptions.SectionName));

        // Register HttpClientFactory for channels
        services.AddHttpClient("LineNotify", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("FCM", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("Twilio", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register notification channels
        services.AddSingleton<INotificationChannel, LineNotifyChannel>();
        services.AddSingleton<INotificationChannel, EmailChannel>();
        services.AddSingleton<INotificationChannel, PushNotificationChannel>();
        services.AddSingleton<INotificationChannel, SmsChannel>();

        // Register supporting services
        services.AddScoped<IRateLimiter, RateLimiter>();
        services.AddScoped<ITemplateEngine, TemplateEngine>();
        services.AddScoped<INotificationService, NotificationService>();

        // Register queue processor as both singleton and hosted service
        services.AddSingleton<NotificationQueueProcessor>();
        services.AddSingleton<INotificationQueueService>(sp => 
            sp.GetRequiredService<NotificationQueueProcessor>());
        services.AddHostedService(sp => 
            sp.GetRequiredService<NotificationQueueProcessor>());

        return services;
    }
}
