using RabbitMQ.Client;

namespace Middleware.Common.EventBus.Abstractions;

public interface IRabbitMqPersistenceConnection : IDisposable
{
    bool IsConnected { get; }

    bool TryConnect();

    IModel CreateModel();
}