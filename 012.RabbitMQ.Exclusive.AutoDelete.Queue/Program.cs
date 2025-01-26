using _012.RabbitMQ.Exclusive.AutoDelete.Queue;


while (true)
{
    Console.Write($"[1] In same connection\n[2] In different connection\n? ");
    string inputResult = Console.ReadLine() ?? string.Empty;

    if (inputResult == "1")
        await new SameConnection().Do();
    else if (inputResult == "2")
        await new DifferentConnections().Do();
    else
        Console.WriteLine("Wrong input!");

    Console.WriteLine();
}