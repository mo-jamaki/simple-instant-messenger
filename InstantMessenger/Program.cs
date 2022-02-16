using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;

namespace InstantMessenger
{
    class Program
    {
        static void Main(string[] args)
        {
            string chatRoomName;
            string name;
            Console.WriteLine("Please Enter the Chat room Name:");
            chatRoomName = Console.ReadLine();
            Console.WriteLine("Please Enter Your Name:");
            name = Console.ReadLine();
            Console.WriteLine("This Chat Count Until write end");
            string line = string.Empty;
            while (line != $"end")
            {
                Console.Write($"{name}:");
                line = Console.ReadLine();
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: "InstantMessenger", type: ExchangeType.Direct);

                        var message = $"{name}:{line}";
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "InstantMessenger",
                                             routingKey: chatRoomName,
                                             basicProperties: null,
                                             body: body);

                    }

                }
                using (var ReceivedConnection = factory.CreateConnection())
                {
                    using (var ReceivedChannel = ReceivedConnection.CreateModel())
                    {
                        ReceivedChannel.ExchangeDeclare(exchange: "InstantMessenger", type: ExchangeType.Direct);

                        var queueName = ReceivedChannel.QueueDeclare().QueueName;
                        ReceivedChannel.QueueBind(queue: queueName,
                                          exchange: "InstantMessenger",
                                          routingKey: chatRoomName);

                        var consumer = new EventingBasicConsumer(ReceivedChannel);
                        consumer.Received += (model, ea) =>
                        {
                            var recivedBody = ea.Body.ToArray();
                            var recivedMessage = Encoding.UTF8.GetString(recivedBody);
                            var arrmessage = recivedMessage.Split(':');
                            if (arrmessage[0] != name)
                            {
                                Console.WriteLine(recivedMessage);
                            }
                            Console.WriteLine(recivedMessage);
                        };
                        ReceivedChannel.BasicConsume(queue: queueName,
                                             autoAck: true,
                                             consumer: consumer);
                    }
                }
            }
        }
    }
}
