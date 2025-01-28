

using RabbitMQ.Client;
using System.Buffers.Binary;
using System.Text;

string[] messages = GetMessages();
var tasks = new List<Task>();
string queue1 = "myq01";
string queue2 = "myq02";

#region Producer
tasks.Add(Task.Run(async () =>
{
    const int MAX_OUTSTANDING_CONFIRMS = 10;
    string exchangeName = "myexch01";
    string routingKey1 = "r.*";
    string routingKey2 = "r.msg";

    var channelOpts = new CreateChannelOptions(
    publisherConfirmationsEnabled: true,
    publisherConfirmationTrackingEnabled: true,
    outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(MAX_OUTSTANDING_CONFIRMS)
    );

    var factory = new ConnectionFactory();
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync(options: channelOpts);

    List<ulong> unsuccessfulMessages = new();

    await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic, autoDelete: false, durable: true);
    await channel.QueueDeclareAsync(queue: queue1, durable: true, exclusive: false, autoDelete: false);
    await channel.QueueDeclareAsync(queue: queue2, durable: true, exclusive: false, autoDelete: false);

    await channel.QueueBindAsync(queue: queue1, exchange: exchangeName, routingKey: routingKey1);
    await channel.QueueBindAsync(queue: queue2, exchange: exchangeName, routingKey: routingKey2);

    channel.BasicAcksAsync += async (sender, eventArgs) =>
    {
        Console.WriteLine($"Ack. Delivery Tag: {eventArgs.DeliveryTag}. (All binding pattern queues that matched the message routing key label received the message)");
    };

    channel.BasicNacksAsync += async (sender, eventArgs) =>
    {
        unsuccessfulMessages.Add(eventArgs.DeliveryTag);
        Console.WriteLine($"Nack. Delivery Tag: {eventArgs.DeliveryTag}. (One or more queues that could have received this message did not receive it)");
    };

    channel.BasicReturnAsync += async (sender, eventArgs) =>
    {
        ulong? seqNo = null;

        if (eventArgs?.BasicProperties != null)
        {
            object? maybeSeqNo = eventArgs?.BasicProperties?.Headers?[Constants.PublishSequenceNumberHeader];
            if (maybeSeqNo != null)
            {
                seqNo = BinaryPrimitives.ReadUInt64BigEndian((byte[])maybeSeqNo);
            }
        }

        Console.WriteLine($"Returned. SeqNo: {seqNo}. (No binding pattern matched the message routing key label)");
    };

    var props = new BasicProperties
    {
        Persistent = true
    };

    var msgsToSend = new List<(ulong? seqNo, string message)>();
    msgsToSend = messages.Select(m => ((ulong?)null, m)).ToList();

    for (int i = 0; i < msgsToSend.Count; ++i)
    {
        try
        {
            var nextChannelPublishSeqNo = await channel.GetNextPublishSequenceNumberAsync();
            var msgToSend = msgsToSend[i];
            msgToSend.seqNo = nextChannelPublishSeqNo;
            string message = $"{msgToSend.message}-{nextChannelPublishSeqNo}";
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: exchangeName,
                routingKey: nextChannelPublishSeqNo % 5 == 0 ? "wrongRK" : "r.msg",
                body: body,
                basicProperties: props,
                mandatory: true);
        }
        catch (Exception exc)
        {
            Console.WriteLine($"[Error]: {exc.Message}");
        }
    }

}));
#endregion


await Task.WhenAll(tasks);

Console.ReadLine();

string[] GetMessages()
{
    List<string> messages = new List<string>();
    const int Message_Count = 20;
    for (int i = 1; i < Message_Count; i++)
    {
        messages.Add($"msg{i}");
    }

    return messages.ToArray();
}