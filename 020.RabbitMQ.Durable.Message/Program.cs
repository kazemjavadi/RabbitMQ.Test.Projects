using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string exchangeName = "myExchange";
string queueName = "queueName";
string routingKey = "rk";

var tasks = new List<Task>();

#region Producer
tasks.Add(Task.Run(async () =>
{
    var factory = new ConnectionFactory();
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();
    //Exchange is durable
    await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

    //Queue is durable
    await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

    await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey);

    var body = Encoding.UTF8.GetBytes("This is message");

    //Setting delivery mode to 2
    BasicProperties basicProperties = new BasicProperties
    {
        DeliveryMode = DeliveryModes.Persistent 
    };

    await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, mandatory: false, body: body, basicProperties: basicProperties );

}));
#endregion

await Task.WhenAll(tasks);
Thread.Sleep(4000);

#region Consumer
string userInput = string.Empty;
do
{
    Console.Write("Do you restart the RabbitMQ server? (y/n) ");
    userInput = Console.ReadLine() ?? string.Empty;
} while (userInput != "y");

var factory = new ConnectionFactory();
var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();

AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender, eventArgs) =>
{
    string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
    Console.WriteLine($"[Received]: {message}");
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.ReadLine();
#endregion