
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost", Port = 5670 };
var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();

var consumer = new AsyncEventingBasicConsumer(channel: channel);

consumer.ReceivedAsync += async (model, eventArgs) =>
{
    string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
    Console.WriteLine($"Received: {message}");
};

string queueName = "q01";
await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.ReadLine();