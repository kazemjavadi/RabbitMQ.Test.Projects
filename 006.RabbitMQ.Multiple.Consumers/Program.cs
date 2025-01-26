
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
var connection = await factory.CreateConnectionAsync();



List<Task> tasks = new List<Task>();

tasks.Add(Task.Run(async() =>
{
    var channel = await connection.CreateChannelAsync();

    var consumer = new AsyncEventingBasicConsumer(channel: channel);

    consumer.ReceivedAsync += async (model, eventArgs) =>
    {
        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"Received [Consumer1]: {message}");
    };

    string queueName = "q01";
    await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

}));

tasks.Add(Task.Run(async () =>
{
    var channel = await connection.CreateChannelAsync();

    var consumer = new AsyncEventingBasicConsumer(channel: channel);

    consumer.ReceivedAsync += async (model, eventArgs) =>
    {
        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"Received [Consumer2]: {message}");
    };

    string queueName = "q01";
    await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

}));

await Task.WhenAll(tasks);

Console.ReadLine();



