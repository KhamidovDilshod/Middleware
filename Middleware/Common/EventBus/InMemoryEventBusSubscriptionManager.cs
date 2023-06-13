using Middleware.Common.EventBus.Abstractions;
using Middleware.Common.EventBus.Models;

namespace Middleware.Common.EventBus;
public partial class InMemoryEventBusSubscriptionManager:IEventBusSubscriptionManager
{

    private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
    private readonly List<Type> _eventTypes;


    public bool IsEmpty { get; }
    public event EventHandler<string>? OnEventRemoved;
    public void AddSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        throw new NotImplementedException();
    }

    public void RemoveSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        throw new NotImplementedException();
    }

    public bool HasSubscriptionForEvent<T>() where T : IntegrationEvent
    {
        throw new NotImplementedException();
    }

    public bool HasSubscriptionForEvent(string eventName)
    {
        throw new NotImplementedException();
    }

    public Type GetEventTypeByName(string eventName)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public string GetEventKey<T>()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName)
    {
        throw new NotImplementedException();
    }
}
public partial class InMemoryEventBusSubscriptionManager:IEventBusSubscriptionManager
{
    public class SubscriptionInfo
    {
        public bool IsDynamic { get; }
        public Type HandlerType { get; }

        private SubscriptionInfo(bool isDynamic, Type handlerType)
        {
            IsDynamic = isDynamic;
            HandlerType = handlerType;
        }

        public static SubscriptionInfo Dynamic(Type handlerType) => new(true, handlerType);
        public static SubscriptionInfo Typed(Type handlerType) => new(false, handlerType);
    }
}