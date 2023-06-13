using Middleware.Common.EventBus.Abstractions;
using RabbitMQ.Client;

namespace Middleware.Common.EventBus;

public static class EventBusExtensions
{
    public static WebApplicationBuilder AddEventBus(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        builder.Services.AddSingleton<IRabbitMqPersistenceConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DefaultRabbitMqPersistenceConnection>>();

            var factory = new ConnectionFactory
            {
                HostName = builder.Configuration["EventBusConnection"],
                DispatchConsumersAsync = true,
                Password = builder.Configuration["EventBusPassword"],
                UserName = builder.Configuration["EventBusUserName"]
            };

            var retryCount = 10;
            return new DefaultRabbitMqPersistenceConnection(factory, logger, retryCount);
        });

        builder.Services.AddSingleton<IEventBus, EventBusRabbitMq>(sp =>
        {
            var subsClientName = builder.Configuration["SubscriptionClientName"];
            var persistenceConnection = sp.GetRequiredService<IRabbitMqPersistenceConnection>();
            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMq>>();
            var retryCount = 5;

            return new EventBusRabbitMq(persistenceConnection, logger, sp, "ELMAtoMQ", retryCount);
        });

        return builder;
    }

    public static void ConfigureEventBus(IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
        eventBus.Subscribe();
    }
}