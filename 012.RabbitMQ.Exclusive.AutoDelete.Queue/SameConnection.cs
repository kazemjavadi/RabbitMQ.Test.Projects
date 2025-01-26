using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace _012.RabbitMQ.Exclusive.AutoDelete.Queue
{
    internal class SameConnection
    {
        public async Task Do()
        {
            string exclusiveQueueName = "myExclusiveQueue";
            string nonExclusiveQueueName = "myNonExclusiveQueue";

            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync();

            List<Task> tasks = new List<Task>();

            //First task
            tasks.Add(Task.Run(async () =>
            {
                try
                {

                    using var channel = await connection.CreateChannelAsync();


                    await channel.QueueDeclareAsync(queue: exclusiveQueueName, durable: false, exclusive: true, autoDelete: false);
                    await channel.QueueDeclareAsync(queue: nonExclusiveQueueName, durable: false, exclusive: false, autoDelete: false);

                    string exchangeName = "myExchange";
                    await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout);

                    await channel.QueueBindAsync(queue: nonExclusiveQueueName, exchange: exchangeName, routingKey: string.Empty);
                    await channel.QueueBindAsync(queue: exclusiveQueueName, exchange: exchangeName, routingKey: string.Empty);

                    var messageBody = Encoding.UTF8.GetBytes("Hello");

                    await channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, body: messageBody);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"[First task]: Error: {exc.Message}");
                }

                Console.WriteLine("[First task]: End.");
            }));

            //Second task
            tasks.Add(Task.Run(async () =>
            {
                Thread.Sleep(4000);

                try
                {
                    using var channel = await connection.CreateChannelAsync();

                    var consumer = new AsyncEventingBasicConsumer(channel: channel);

                    consumer.ReceivedAsync += async (model, eventArgs) =>
                    {
                        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                        Console.WriteLine($"[Second]: Received: {message}");
                    };

                    await channel.BasicConsumeAsync(queue: exclusiveQueueName, autoAck: true, consumer: consumer);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"[Second task]: Error: {exc.Message}");
                }

                Console.WriteLine("[Second task]: End.");
            }));

            //Third task
            tasks.Add(Task.Run(async () =>
            {
                Thread.Sleep(4000);

                try
                {
                    var channel = await connection.CreateChannelAsync();

                    var consumer = new AsyncEventingBasicConsumer(channel: channel);

                    consumer.ReceivedAsync += async (model, eventArgs) =>
                    {
                        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                        Console.WriteLine($"[Third task]: Received: {message}");
                    };

                    await channel.BasicConsumeAsync(queue: nonExclusiveQueueName, autoAck: true, consumer: consumer);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"[Third task]: Error: {exc.Message}");
                }

                Console.WriteLine("[Third task]: End.");
            }));

            await Task.WhenAll(tasks);
        }
    }
}
