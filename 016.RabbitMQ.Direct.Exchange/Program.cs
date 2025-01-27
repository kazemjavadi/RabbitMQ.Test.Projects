

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string exchangeName = "myDirectExchange";
string queueName1 = "myQueue1";
string queueName2 = "myQueue2";
string routkingKey1 = "rk1";
string routkingKey2 = "rk2";

var tasks = new List<Task>();

#region Producer
tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct);
    await channel.QueueDeclareAsync(queue: queueName1, exclusive: false);
    await channel.QueueDeclareAsync(queue: queueName2, exclusive: false);

    await channel.QueueBindAsync(queue: queueName1, exchange: exchangeName, routingKey: routkingKey1);
    await channel.QueueBindAsync(queue: queueName2, exchange: exchangeName, routingKey: routkingKey2);

    for (int i = 0; i < 10; i++)
    {
        var body1 = Encoding.UTF8.GetBytes($"This is message number {i} with routing key {routkingKey1}");
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routkingKey1, body1);

        var body2 = Encoding.UTF8.GetBytes($"This is message number {i} with routing key {routkingKey2}");
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routkingKey2, body2);
    }
}));
#endregion

#region Consumer 1
tasks.Add(Task.Run(async () =>
{
    Thread.Sleep(5000);

    var factory = new ConnectionFactory { HostName = "localhost" };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"[Queue: {queueName1} - RoutingKey: {routkingKey1}]: {message}");
    };

    await channel.BasicConsumeAsync(queue: queueName1, autoAck: true, consumer: consumer);
}));
#endregion


#region Consumer 2
tasks.Add(Task.Run(async () =>
{
    Thread.Sleep(5000);

    var factory = new ConnectionFactory { HostName = "localhost" };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"[Queue: {queueName2} - RoutingKey: {routkingKey2}]: {message}");
    };

    await channel.BasicConsumeAsync(queue: queueName2, autoAck: true, consumer: consumer);
}));
#endregion

await Task.WhenAll(tasks);
Console.ReadLine();