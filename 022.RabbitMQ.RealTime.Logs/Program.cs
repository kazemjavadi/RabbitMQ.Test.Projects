

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
var connection = await factory.CreateConnectionAsync();

string rabbitmqLogsExchagneName = "amq.rabbitmq.log";

string errorLogsQueueName = "errorLogsQueue";
string warningLogsQueueName = "warningLogsQueue";
string infoLogsQueueName = "infoLogsQueue";

string errorRoutingKey = "error";
string warningRoutingKey = "warning";
string infoRoutingKey = "info";

var tasks = new List<Task>();

#region Error logs consumer
tasks.Add(Task.Run(async () =>
{
    var channel = await connection.CreateChannelAsync();
    await channel.QueueDeclareAsync(queue: errorLogsQueueName);
    await channel.QueueBindAsync(queue: errorLogsQueueName, exchange: rabbitmqLogsExchagneName, routingKey: errorRoutingKey);

    AsyncEventingBasicConsumer consumer = new(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        string errorMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        WriteLineRed($"[Error]: {errorMessage}");  
    };

    await channel.BasicConsumeAsync(queue: errorLogsQueueName, autoAck: true, consumer);
}));
#endregion

#region Warning logs consumer
tasks.Add(Task.Run(async () =>
{
    var channel = await connection.CreateChannelAsync();
    await channel.QueueDeclareAsync(queue: warningLogsQueueName);
    await channel.QueueBindAsync(queue: warningLogsQueueName, exchange: rabbitmqLogsExchagneName, routingKey: warningRoutingKey);

    AsyncEventingBasicConsumer consumer = new(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        string errorMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        WriteLineYellow($"[Warning]: {errorMessage}");
    };

    await channel.BasicConsumeAsync(queue: warningLogsQueueName, autoAck: true, consumer);

}));
#endregion

#region Information logs consumer
tasks.Add(Task.Run(async () =>
{
    var channel = await connection.CreateChannelAsync();
    await channel.QueueDeclareAsync(queue: infoLogsQueueName);
    await channel.QueueBindAsync(queue: infoLogsQueueName, exchange: rabbitmqLogsExchagneName, routingKey: infoRoutingKey);

    AsyncEventingBasicConsumer consumer = new(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        string errorMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        WriteLineBlue($"[Info]: {errorMessage}");
    };

    await channel.BasicConsumeAsync(queue: infoLogsQueueName, autoAck: true, consumer);

}));
#endregion

await Task.WhenAll(tasks);
Console.ReadLine();

void WriteLineRed(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(msg);
    Console.ForegroundColor = ConsoleColor.White;
}

void WriteLineYellow(string msg)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(msg);
    Console.ForegroundColor = ConsoleColor.White;
}

void WriteLineBlue(string msg)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine(msg);
    Console.ForegroundColor = ConsoleColor.White;
}