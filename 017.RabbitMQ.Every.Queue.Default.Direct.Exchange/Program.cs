

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string queueName = "myQueue";

var tasks = new List<Task>();

#region Producer
tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(queue: queueName, exclusive: false);

    for (int i = 0; i < 10; i++)
    {
        var body = Encoding.UTF8.GetBytes($"This is message number {i}");
        //exchange name is empty string
        //routing key is queue name
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
    }
}));
#endregion

#region Consumer 
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
        Console.WriteLine($"[Received]: {message}");
    };

    await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
}));
#endregion

await Task.WhenAll(tasks);
Console.ReadLine();