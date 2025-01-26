using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();

var consumer = new AsyncEventingBasicConsumer(channel: channel);

consumer.ReceivedAsync += async (model, eventArgs) =>
{
    string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
    Console.WriteLine($"Received: {message}");

    if (message.Contains("number 9"))
        await channel.BasicRejectAsync(eventArgs.DeliveryTag, requeue: false);
    else
        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
};

string queueName = "q01";
await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

Console.ReadLine();