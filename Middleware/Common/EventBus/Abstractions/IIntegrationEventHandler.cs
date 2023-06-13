using Middleware.Common.EventBus.Models;

namespace Middleware.Common.EventBus.Abstractions;

public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
{
    Task HandleAsync(TIntegrationEvent integrationEvent);
}

public interface IIntegrationEventHandler
{
}