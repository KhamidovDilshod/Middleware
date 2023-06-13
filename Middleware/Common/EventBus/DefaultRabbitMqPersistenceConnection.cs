using System.Net.Sockets;
using Middleware.Common.EventBus.Abstractions;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Middleware.Common.EventBus;

public class DefaultRabbitMqPersistenceConnection : IRabbitMqPersistenceConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<DefaultRabbitMqPersistenceConnection> _logger;
    private readonly int _retryCount;
    private IConnection _connection;
    private bool Disposed;

    private readonly object _syncRoot = new();


    public DefaultRabbitMqPersistenceConnection(IConnectionFactory connectionFactory,
        ILogger<DefaultRabbitMqPersistenceConnection> logger, int retryCount)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _retryCount = retryCount;

        Thread.Sleep(3000);
    }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        try
        {
            _connection.ConnectionShutdown -= OnConnectionShutDown;
            _connection.CallbackException -= OnCallbackException;
            _connection.ConnectionBlocked -= OnConnectionBlocked;
            _connection.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex.ToString());
        }
    }

    public bool IsConnected => _connection is { IsOpen: true } && !Disposed;

    public bool TryConnect()
    {
        _logger.LogInformation("RabbitMQ client is trying to connect");

        lock (_syncRoot)
        {
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning("RabbitMQ client could not connect after {TimeOut}s ({ExceptionMessage})",
                            $"{time.TotalSeconds:n1}", ex.Message);
                    });
            policy.Execute(() => _connection = _connectionFactory.CreateConnection());
        }

        if (IsConnected)
        {
            _connection.ConnectionShutdown += OnConnectionShutDown;
            _connection.CallbackException += OnCallbackException;
            _connection.ConnectionBlocked += OnConnectionBlocked;

            _logger.LogInformation(
                "RabbitMQ Client acquired acquired a persistent connection to '{HostName}' and is subscribed to failure events",
                _connection.Endpoint.HostName);

            return true;
        }
        else
        {
            _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
            return false;
        }
    }

    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections available to perform this action");
        }

        return _connection.CreateModel();
    }

    void OnConnectionShutDown(object sender, ShutdownEventArgs reason)
    {
        if (Disposed) return;
        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect");

        TryConnect();
    }

    void OnCallbackException(object sender, CallbackExceptionEventArgs eventArgs)
    {
        if (Disposed) return;
        _logger.LogWarning("A RabbitMQ connection thrown an exception. Trying to re-connect");

        TryConnect();
    }

    void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs eventArgs)
    {
        if (Disposed) return;
        _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect");

        TryConnect();
    }
}