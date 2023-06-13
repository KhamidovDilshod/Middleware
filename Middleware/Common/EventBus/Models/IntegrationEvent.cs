using System.Text.Json.Serialization;

namespace Middleware.Common.EventBus.Models;

public record IntegrationEvent
{
    [JsonInclude] public Guid Id { get; private set; }
    [JsonInclude] public DateTime CreationDate { get; private set; }

    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime creationDate)
    {
        Id = id;
        CreationDate = creationDate;
    }
}