using System.Text;
using Middleware.Common.EventBus.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Middleware.Common.EventBus;

public class EventBusRabbitMq : IEventBus
{
    private const string BROKER_NAME = "ExELMAtoMQ";

    private readonly IRabbitMqPersistenceConnection _persistenceConnection;
    private readonly ILogger<EventBusRabbitMq> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _retryCount;

    private IModel _consumerChannel;
    private string _queueName;

    public EventBusRabbitMq(IRabbitMqPersistenceConnection persistenceConnection, ILogger<EventBusRabbitMq> logger,
        IServiceProvider serviceProvider, string queueName = null, int retryCount = 5)
    {
        _persistenceConnection = persistenceConnection;
        _logger = logger;
        _queueName = queueName;
        _consumerChannel = CreateConsumerChannel();
        _serviceProvider = serviceProvider;
        _retryCount = retryCount;
    }

    private IModel CreateConsumerChannel()
    {
        if (!_persistenceConnection.IsConnected)
        {
            _persistenceConnection.TryConnect();
        }

        _logger.LogTrace("Creating RabbitMQ consumer channel");

        var channel = _persistenceConnection.CreateModel();

        channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");

        if (!IsQueueExists(channel))
        {
            channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: true,
                autoDelete: false,
                arguments: null);
        }
        
        channel.CallbackException += (sender, eventArgs) =>
        {
            _logger.LogWarning(eventArgs.Exception, "Recreating RabbitMQ consumer channel");

            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        };

        return channel;
    }

    private void StartBasicConsume()
    {
        _logger.LogTrace("Starting RabbitMQ basic consume");

        if (_consumerChannel != null)
        {
            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }
        else
        {
            _logger.LogError("StartBasicConsume can't call on _consumerChannel==null");
        }
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
    {
        var eventName = @event.RoutingKey;
        var message = Encoding.UTF8.GetString(@event.Body.ToArray());

        try
        {
            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"---- ERROR Processing message \"{message}");
        }

        _consumerChannel.BasicAck(@event.DeliveryTag, multiple: false);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

        try
        {
            _logger.LogInformation($"{eventName} with message {message}");
        }
        catch (Exception exception)
        {
            _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
        }

        await Task.CompletedTask;
    }

    public void Publish(object @event)
    {
        throw new NotImplementedException();
    }

    public void Subscribe()
    {
        var eventName = "ELMAtoMQ";
        StartBasicConsume();
    }

    private bool IsQueueExists(IModel channel)
    {
        try
        {
            channel.QueueDeclarePassive(_queueName);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}