



using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;

var tasks = new List<Task>();

#region Producer
tasks.Add(Task.Run(async () =>
{
//    var factory = new ConnectionFactory() { HostName = "localhost", Port = 5670 };
//    var connection = await factory.CreateConnectionAsync();
//    var channel = await connection.CreateChannelAsync();

//    // Declare a mirrored queue
//    Dictionary<string, object?> arguments = new()
//    {
//        { "x-ha-policy", "all" }  // Enables mirroring for this queue
//    };

//    await channel.QueueDeleteAsync(queue: "myMirroredQueue");
//    await channel.QueueDeleteAsync(queue: "myNonMirroredQueue");

//    await channel.QueueDeclareAsync(queue: "myMirroredQueue", exclusive: false, arguments: arguments);
//    await channel.QueueDeclareAsync(queue: "myNonMirroredQueue", exclusive: false);

//    await channel.ExchangeDeclareAsync(exchange: "myExchange", type: ExchangeType.Direct);
//    await channel.QueueBindAsync(queue: "myNonMirroredQueue", exchange: "myExchange", routingKey: string.Empty);

//    await channel.BasicPublishAsync(exchange: "myExchange", routingKey: string.Empty, Encoding.UTF8.GetBytes("Hello"));
}));
#endregion

#region Consumer
tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory() { HostName = "localhost", Port = 5670 };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        Console.WriteLine(Encoding.UTF8.GetString(eventArgs.Body.ToArray()));
    };

    await channel.BasicConsumeAsync(queue: "myMirroredQueue", autoAck: true, consumer: consumer);
}));
#endregion

await Task.WhenAll(tasks);

Console.WriteLine("Run this \"rabbitmqctl list_queues name policy slave_pids\" command to check your queue is mirrored. ");

Console.ReadLine();