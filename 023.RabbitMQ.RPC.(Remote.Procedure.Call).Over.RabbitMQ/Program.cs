

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var tasks = new List<Task>();

#region RPC server
tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory();
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    await channel.ExchangeDeclareAsync(exchange: "rpc", type: ExchangeType.Direct, autoDelete: false);
    await channel.QueueDeclareAsync(queue: "ping", autoDelete: false);
    await channel.QueueBindAsync(queue: "ping", exchange: "rpc", routingKey: "ping");

    AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"[RPC Server]: Message from client received. The message is: {message}");
        string? replyTo = eventArgs?.BasicProperties?.ReplyTo;
        var replyBody = Encoding.UTF8.GetBytes($"Pong {DateTime.Now}");
        if (replyTo != null)
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: replyTo, body: replyBody);
    };

    await channel.BasicConsumeAsync(queue: "ping", autoAck: true, consumer);
}));
#endregion

#region RPC client
tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory();
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    var body = Encoding.UTF8.GetBytes($"Ping {DateTime.Now}");

    var queueDeclareResult = await channel.QueueDeclareAsync(exclusive: true, autoDelete: true);
    BasicProperties properties = new BasicProperties
    {
        ReplyTo = queueDeclareResult.QueueName
    };

    await channel.BasicPublishAsync(exchange: "rpc", routingKey: "ping", basicProperties: properties, body: body, mandatory: false);

    AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"[PRC Client]: Message from server received. The message is: {message}");
        await channel.BasicCancelAsync(consumerTag: eventArgs.ConsumerTag);
    };

    await channel.BasicConsumeAsync(queue: queueDeclareResult.QueueName, autoAck: true, consumer: consumer);

}));
#endregion

await Task.WhenAll(tasks);

Console.ReadLine();
