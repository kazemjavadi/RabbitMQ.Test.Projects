
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string exchangeName = "exch01";
string queueName = "q01";
var tasks = new List<Task>();

#region Producer

tasks.Add(Task.Run(async () =>
{
    while (true)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = "localhost", Port = 5670 };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false);

            var arguments = new Dictionary<string, object?>
            {
                { "x-queue-type", "quorum" } // Set the queue type to Quorum
            };

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

            await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty);

            Console.WriteLine($"[Producer]> Producing messages...");
            for (int i = 0; i < 2_000_000_000; i++)
            {
                Thread.Sleep(700);
                string message = $"This is message number {i} - {DateTime.Now}";
                byte[] body = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, body: body);
            }

            Console.ReadLine();
        }
        catch (Exception exc)
        {
            Console.WriteLine($"[Producer]> Error: {exc.Message}");
            Console.WriteLine("[Consumer]> Surviving failure...");
            Thread.Sleep(4000);
        }
    }
}));

#endregion

#region Consumer

tasks.Add(Task.Run(async () =>
{
    while (true)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = "localhost", Port = 5670 };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            Console.WriteLine("[Consumer]> Waitng for new messages...");
            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                //Thread.Sleep(1000);
                string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                Console.WriteLine($"[Consumer]> Received: {message}");
                await channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            Console.ReadLine();
        }
        catch (Exception exc)
        {
            Console.WriteLine($"[Consumer]> Error: {exc.Message}");
            Console.WriteLine("[Consumer]> Surviving failure...");
            Thread.Sleep(5000);
        }
    }
}));

#endregion

await Task.WhenAll(tasks);

Console.ReadLine();