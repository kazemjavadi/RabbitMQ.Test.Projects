using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

await Scenario1();
await Scenario2();

//Exhange with no queue
async Task Scenario1()
{
    var factory = new ConnectionFactory { HostName = "localhost" };
    var connection = await factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    string exchangeName = "exch01";
    await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic);

    var messageBody = Encoding.UTF8.GetBytes("This is my message.");
    await channel.BasicPublishAsync(exchange: exchangeName, string.Empty, body: messageBody);
}

//Message have a routing key that don't match any of binding patterns
async Task Scenario2()
{

    var factory = new ConnectionFactory { HostName = "localhost" };
    var connection = await factory.CreateConnectionAsync();

    var tasks = new List<Task>();

    //Producer
    tasks.Add(Task.Run(async () =>
    {
        var channel = await connection.CreateChannelAsync();

        string exchangeName = "exch01";
        string queueName1 = "Q1";
        string queueName2 = "Q2";

        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic);
        await channel.QueueDeclareAsync(queue: queueName1);
        await channel.QueueDeclareAsync(queue: queueName2);
        await channel.QueueBindAsync(queue: queueName1, exchange: exchangeName, routingKey: "a.*");
        await channel.QueueBindAsync(queue: queueName2, exchange: exchangeName, routingKey: "b.*");

        var body1 = Encoding.UTF8.GetBytes("This message is going to the Q1");
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "a.msg", body: body1);

        var body2 = Encoding.UTF8.GetBytes("This is message is going to the Q2");
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "b.msg", body: body2);

        var body3 = Encoding.UTF8.GetBytes("This will be black-holed");
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "c.msg", body: body1);
    }));

    //Consumer 1
    tasks.Add(Task.Run(async () =>
    {
        Thread.Sleep(3000);

        var channel = await connection.CreateChannelAsync();

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            Console.WriteLine($"[Q1 - Routing Key: a.*]: [Received Message]: {message}");
        };

        await channel.BasicConsumeAsync(queue: "Q1", autoAck: true, consumer: consumer);

    }));


    //Consumer 2
    tasks.Add(Task.Run(async () =>
    {
        Thread.Sleep(3000);

        var channel = await connection.CreateChannelAsync();

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            Console.WriteLine($"[Q2 - Routing Key: b.*]: [Received Message]: {message}");
        };

        await channel.BasicConsumeAsync(queue: "Q2", autoAck: true, consumer: consumer);

    }));

    await Task.WhenAll(tasks);

    Console.ReadLine();
}
