using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


Console.Write("[1] RabbitMQ basic.consume\n[2] RabbitMQ. basic.get\n? ");
string userInput = Console.ReadLine() ?? string.Empty;

if (userInput == "1")
    await RabbitMQBasicConsume();
else if (userInput == "2")
    await RabbitMQBasicGet();
else
    Console.WriteLine("Wrong input!");

async Task RabbitMQBasicGet()
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();
    string queueName = "q01";

    while (true)
    {
        var result = await channel.BasicGetAsync(queue: queueName, true);
        if (result == null) continue;

        var message = Encoding.UTF8.GetString(result.Body.ToArray());
        Console.WriteLine($"Received: {message}");
    }
}

async Task RabbitMQBasicConsume()
{
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
    await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

    Console.ReadLine();
}