
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string exchangeName = "exch01";
string queueName = "q01";
var tasks = new List<Task>();

tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory { HostName = "localhost", Port = 5670 };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false);

    var arguments = new Dictionary<string, object?>
    {
        { "x-queue-type", "quorum" } // Set the queue type to Quorum
    };

    await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

    await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty);

    for (int i = 0; i < 2_000_000_000; i++)
    {
        string message = $"This is message number {i} - {DateTime.Now}";
        byte[] body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, body: body);
    }
}));

tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory { HostName = "localhost", Port = 5670 };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        Thread.Sleep(200);
        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"Received: {message}");
    };

    await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
}));

await Task.WhenAll(tasks);

Console.ReadLine();