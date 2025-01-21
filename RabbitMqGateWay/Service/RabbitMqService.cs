using RabbitMQ.Client;
using System;
using System.Text;
using RabbitMQ.Client.Events;

public class RabbitMqProducerService
{
    public RabbitMqProducerService()
    {
    }

    public async Task SendMessageAsync(string message)
    {
        string _hostName = "localhost"; // or the RabbitMQ server address
        string _queueName = "queuename_sendemail";
        try
        {
            var factory = new ConnectionFactory() { HostName = _hostName };

            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                // Declare the queue
                await channel.QueueDeclareAsync(queue: _queueName,
                                                durable: false,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);
                
                // Convert the message to bytes
                var body = Encoding.UTF8.GetBytes(message);

                // Create basic properties
                var properties = new BasicProperties();

                // Publish the message
                await channel.BasicPublishAsync(exchange: "",
                                                routingKey: _queueName,
                                                mandatory: false,
                                                basicProperties: properties,
                                                body: body);

                Console.WriteLine($"Message sent: {message}");
               
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            throw; // Re-throw the exception for handling by the caller
        }
    }

    public async Task SendNotify(string message)
    {
        string _hostName = "localhost"; // or the RabbitMQ server address
        string _queueName = "queuename_sendnoti";
        try
        {
            var factory = new ConnectionFactory() { HostName = _hostName };
                        
            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                // Declare the queue
                await channel.QueueDeclareAsync(queue: _queueName,
                                                durable: false,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);

                // Convert the message to bytes
                var body = Encoding.UTF8.GetBytes(message);
                /*await channel.ExchangeDeclareAsync("notify_exchange", ExchangeType.Direct);*/
                // Create basic properties
                var properties = new BasicProperties();

                // Publish the message
                await channel.BasicPublishAsync(exchange: "",
                                                routingKey: _queueName,
                                                mandatory: false,
                                                basicProperties: properties,
                                                body: body);

                Console.WriteLine($"Message sent: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            throw; // Re-throw the exception for handling by the caller
        }
    }

    public async Task<string> ConsumeMessageAsync()
    {
        string _hostName = "localhost"; // or the RabbitMQ server address
        string _queueName = "queuename_sendemail";
        try
        {
            var factory = new ConnectionFactory() { HostName = _hostName };

            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                // Declare the queue
                await channel.QueueDeclareAsync(queue: _queueName,
                                                durable: false,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);

                var consumer = new AsyncEventingBasicConsumer(channel);
                var tcs = new TaskCompletionSource<string>();

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    tcs.SetResult(message);
                    await Task.Yield(); // Ensure the event handler is awaited
                };

                await channel.BasicConsumeAsync(queue: _queueName,
                                                autoAck: true,
                                                consumer: consumer);

                return await tcs.Task;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error consuming message: {ex.Message}");
            throw; // Re-throw the exception for handling by the caller
        }
    }
}
