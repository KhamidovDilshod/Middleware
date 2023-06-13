using Middleware.Common.EventBus.Models;

namespace Middleware.Common.EventBus.Abstractions;

public interface IEventBus
{
    public void Publish(object @event);

    // public void Subscribe<T, TH>()
    //     where T : IntegrationEvent
    //     where TH : IIntegrationEventHandler<T>;
    public void Subscribe();
}