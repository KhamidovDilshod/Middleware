using Middleware.Common.EventBus.Models;

namespace Middleware.Common.EventBus.Abstractions;

public interface IEventBusSubscriptionManager
{
    bool IsEmpty { get; }
    event EventHandler<string> OnEventRemoved;

    void AddSubscription<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    void RemoveSubscription<T, TH>()
        where TH : IIntegrationEventHandler<T>
        where T : IntegrationEvent;

    bool HasSubscriptionForEvent<T>() where T : IntegrationEvent;

    bool HasSubscriptionForEvent(string eventName);

    Type GetEventTypeByName(string eventName);

    void Clear();

    string GetEventKey<T>();
    
    IEnumerable<InMemoryEventBusSubscriptionManager.SubscriptionInfo> GetHandlersForEvent(string eventName);
}