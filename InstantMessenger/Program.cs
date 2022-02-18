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
            string exchangeName = "InstantMessenger";
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = name, Password = name };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
                    var queueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(queue: queueName,
                                      exchange: exchangeName,
                                      routingKey: chatRoomName);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var recivedBody = ea.Body.ToArray();
                        var recivedMessage = Encoding.UTF8.GetString(recivedBody);
                        var routingKey = ea.RoutingKey;
                        Console.WriteLine($"{ea.BasicProperties.UserId}:{recivedMessage}");
                    };
                    channel.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);

                    IBasicProperties props = channel.CreateBasicProperties();
                    props.UserId = name;

                    while (line != $"end")
                    {
                        //Console.Write($"{name}:");
                        line = Console.ReadLine();
                        var message = line;
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: exchangeName,
                                             routingKey: chatRoomName,
                                             basicProperties: props,
                                             body: body);
                    }
                }
            }
        }
    }
}
