using Newtonsoft.Json;

namespace PolicyService.Messaging.RabbitMq.Outbox;

public class Message
{
    protected Message()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    public Message(object message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        Type = message.GetType().FullName + ", " + message.GetType().Assembly.GetName().Name;
        Payload = JsonConvert.SerializeObject(message);
    }

    public virtual long? Id { get; protected set; }
    public virtual string Type { get; protected set; }
    public virtual string Payload { get; protected set; }

    public virtual object RecreateMessage()
    {
        return JsonConvert.DeserializeObject(Payload, System.Type.GetType(Type)!)!;
    }
}
