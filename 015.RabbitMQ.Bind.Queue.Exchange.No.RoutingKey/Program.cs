

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


string exchangeName = "exch";
string queueName1 = "mq1";
string queueName2 = "mq2";
string routingKey1 = "4b.*";
string emptyRoutingKey = string.Empty;

var tasks = new List<Task>
{
    //Producer
    Task.Run(async () =>
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic);
        await channel.QueueDeclareAsync(queue: queueName1, exclusive: false, autoDelete: false);
        await channel.QueueDeclareAsync(queue: queueName2, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(queue: queueName1, exchange: exchangeName, routingKey: routingKey1);
        await channel.QueueBindAsync(queue: queueName2, exchange: exchangeName, routingKey: emptyRoutingKey);

        for(int i = 0; i < 6; ++i)
        {
            var body1 = Encoding.UTF8.GetBytes($"This is my message number {i}.");
            var body2 = Encoding.UTF8.GetBytes($"This is my message number {i}. For no routing key.");
            await channel.BasicPublishAsync(exchange: exchangeName, routingKey: $"{i}b.a*", body: body1);
            await channel.BasicPublishAsync(exchange: exchangeName, routingKey: emptyRoutingKey, body: body2);
        }
    }),


    //Consumer 1
    Task.Run(async () =>
    {
        Thread.Sleep(4000);

        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            Console.WriteLine($"[Queue: q1 | Routing Key: {routingKey1}]: [Message]: {message}");
        };

        await channel.BasicConsumeAsync(queue: queueName1, autoAck: true, consumer: consumer);
    }),

    //Consumer 2
    Task.Run(async () =>
    {
        Thread.Sleep(4000);

        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            Console.WriteLine($"[Queue: q2 | Routing Key: {emptyRoutingKey}]: [Message]: {message}");
        };

        await channel.BasicConsumeAsync(queue: queueName2, autoAck: true, consumer: consumer);
    })
};

await Task.WhenAll(tasks);
Console.ReadLine();