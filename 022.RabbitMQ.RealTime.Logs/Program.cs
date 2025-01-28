

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
var connection = await factory.CreateConnectionAsync();

string rabbitmqLogsExchagneName = "amq.rabbitmq.log";

string errorRoutingKey = "error";
string warningRoutingKey = "warning";
string infoRoutingKey = "info";

var tasks = new List<Task>();

#region Error logs consumer
tasks.Add(Task.Run(async () =>
{
    var channel = await connection.CreateChannelAsync();
    var queueDeclareResult = await channel.QueueDeclareAsync();
    await channel.QueueBindAsync(queue: queueDeclareResult.QueueName, exchange: rabbitmqLogsExchagneName, routingKey: errorRoutingKey);

    AsyncEventingBasicConsumer consumer = new(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        string errorMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        WriteLineRed($"[Error]: {errorMessage}");  
    };

    await channel.BasicConsumeAsync(queue: queueDeclareResult.QueueName, autoAck: true, consumer);
}));
#endregion

#region Warning logs consumer
tasks.Add(Task.Run(async () =>
{
    var channel = await connection.CreateChannelAsync();
    var queueDeclareResult = await channel.QueueDeclareAsync();
    await channel.QueueBindAsync(queue: queueDeclareResult.QueueName, exchange: rabbitmqLogsExchagneName, routingKey: warningRoutingKey);

    AsyncEventingBasicConsumer consumer = new(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        string errorMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        WriteLineYellow($"[Warning]: {errorMessage}");
    };

    await channel.BasicConsumeAsync(queue: queueDeclareResult.QueueName, autoAck: true, consumer);

}));
#endregion

#region Information logs consumer
tasks.Add(Task.Run(async () =>
{
    var channel = await connection.CreateChannelAsync();
    var queueDeclareResult = await channel.QueueDeclareAsync();
    await channel.QueueBindAsync(queue: queueDeclareResult.QueueName, exchange: rabbitmqLogsExchagneName, routingKey: infoRoutingKey);

    AsyncEventingBasicConsumer consumer = new(channel);
    consumer.ReceivedAsync += async (sender, eventArgs) =>
    {
        string errorMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        WriteLineBlue($"[Info]: {errorMessage}");
    };

    await channel.BasicConsumeAsync(queue: queueDeclareResult.QueueName, autoAck: true, consumer);

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