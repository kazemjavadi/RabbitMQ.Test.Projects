using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

//Classic mirrored queues were deprecated in RabbitMQ version 3.9.
//They will be removed completely in RabbitMQ version 4.0.

string mirroredQueueName = "myMirroredQueue";
string nonMirroredQueueName = "myNonMirroredQueue";
string exchangeName = "myExchange";
string userInput = string.Empty;

while (true)
{
    do
    {
        Console.Write("[1] Producer [2] Consumer (Mirrored queue) [3] Consumer (None mirrored queue): ");
        userInput = Console.ReadLine() ?? string.Empty;
    } while (!(userInput == "1" || userInput == "2" || userInput == "3"));

    if (userInput == "1") //Producer
    {

        var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeleteAsync(queue: mirroredQueueName);
        await channel.QueueDeleteAsync(queue: nonMirroredQueueName);

        // Declare a mirrored queue
        Dictionary<string, object?> arguments = new()
        {
            { "ha-mode", "all" }  // Enables mirroring for this queue
        };

        await channel.QueueDeclareAsync(queue: mirroredQueueName, exclusive: false, arguments: arguments);
        await channel.QueueDeclareAsync(queue: nonMirroredQueueName, exclusive: false);

        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct);
        await channel.QueueBindAsync(queue: mirroredQueueName, exchange: exchangeName, routingKey: string.Empty);
        await channel.QueueBindAsync(queue: nonMirroredQueueName, exchange: exchangeName, routingKey: string.Empty);

        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, Encoding.UTF8.GetBytes("Hello"));

        Thread.Sleep(3000);
    }
    else if (userInput == "2")
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5670 };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                Console.WriteLine($"[Mirrored queue]: {message}");
            };

            await channel.BasicConsumeAsync(queue: mirroredQueueName, autoAck: true, consumer: consumer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error-{mirroredQueueName}]: {ex.Message}");
        }
    }
    else if (userInput == "3")
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5670 };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                Console.WriteLine($"[{nonMirroredQueueName}]: {message}");
            };

            await channel.BasicConsumeAsync(queue: nonMirroredQueueName, autoAck: true, consumer: consumer);
        }
        catch (Exception exc)
        {
            Console.WriteLine($"[Error-{nonMirroredQueueName}]: {exc.Message}");
        }
    }
}