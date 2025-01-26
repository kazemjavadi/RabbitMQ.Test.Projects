using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Security.AccessControl;

var factory = new ConnectionFactory { HostName = "localhost" };

var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();

string queueName = "myQ_abc";

await channel.QueueDeclareAsync(queue: queueName, autoDelete: true);

//Consumer
var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender, eventArgs) => { };
await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

await CheckQueueExist();

await channel.CloseAsync();
await connection.CloseAsync();

connection = await factory.CreateConnectionAsync();
channel = await connection.CreateChannelAsync();

await CheckQueueExist();

async Task CheckQueueExist()
{
    try
    {
        await channel.QueueDeclareAsync(queue: queueName, autoDelete: true, durable: false, exclusive: false, passive: true);
        Console.WriteLine("Queue exist.");
    }
    catch (Exception exc)
    {
        Console.WriteLine($"Queue not exist. Error: {exc.Message}");
    }
}