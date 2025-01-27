

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string exchangeName = "myTopicExchange";
string queueName1 = "myQueue1";
string queueName2 = "myQueue2";
string queueName3 = "myQueue3";
string routkingKey1 = "rk1.*";
string routkingKey2 = "rk2.*";
string routkingKey3 = "#";

var tasks = new List<Task>();

#region Producer
tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic);
    await channel.QueueDeclareAsync(queue: queueName1, exclusive: false);
    await channel.QueueDeclareAsync(queue: queueName2, exclusive: false);
    await channel.QueueDeclareAsync(queue: queueName3, exclusive: false);

    await channel.QueueBindAsync(queue: queueName1, exchange: exchangeName, routingKey: routkingKey1);
    await channel.QueueBindAsync(queue: queueName2, exchange: exchangeName, routingKey: routkingKey2);
    await channel.QueueBindAsync(queue: queueName3, exchange: exchangeName, routingKey: routkingKey3);

    for (int i = 0; i < 3; i++)
    {
        var body1 = Encoding.UTF8.GetBytes($"A{i} with routing key {routkingKey1}");
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routkingKey1, body1);

        var body2 = Encoding.UTF8.GetBytes($"B{i} with routing key {routkingKey2}");
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routkingKey2, body2);


        //var body3 = Encoding.UTF8.GetBytes($"This is message number {i} with routing key {routkingKey3}");
        //await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routkingKey3, body3);
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

#region Consumer 3
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
        Console.WriteLine($"[Queue: {queueName3} - RoutingKey: {routkingKey3}]: {message}");
    };

    await channel.BasicConsumeAsync(queue: queueName3, autoAck: true, consumer: consumer);
}));
#endregion

await Task.WhenAll(tasks);
Console.ReadLine();