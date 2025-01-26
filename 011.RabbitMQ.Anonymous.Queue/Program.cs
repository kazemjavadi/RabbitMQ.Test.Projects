using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

var result = await channel.QueueDeclareAsync(queue: string.Empty);

Console.WriteLine($"Queue is created. Queue name is \"{result.QueueName}\"");
Console.ReadLine();