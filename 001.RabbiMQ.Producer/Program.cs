
using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

string exchangeName = "ex01";
await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);
string queueName = "q01";
await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty);

for (int i = 0; i < 20; i++)
{
    string message = $"This is message number {i} from Producer ({DateTime.Now})";
    var messageBody = Encoding.UTF8.GetBytes(message);

    Console.WriteLine($"Message: {message}");


    Thread.Sleep(new Random().Next(1000, 5000));

    await channel.BasicPublishAsync(exchange: exchangeName,
        routingKey: string.Empty,
        body: messageBody);
}

await channel.CloseAsync();
Console.ReadLine();