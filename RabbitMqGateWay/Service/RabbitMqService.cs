using RabbitMQ.Client;
using System;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMqGateWay.Controllers;
using System.Text.Json;

public class RabbitMqProducerService
{
    public RabbitMqProducerService()
    {
    }

    public async Task SendMessageAsync(Request request)
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

                // Serialize the request object to JSON
                var message = JsonSerializer.Serialize(request);
                var body = Encoding.UTF8.GetBytes(message);

                // Create basic properties and add retryCount header
                var properties = new BasicProperties();
                properties.Headers = new Dictionary<string, object> {{ "retryCount",0}};
                // Publish the message
                await channel.BasicPublishAsync(exchange: "",
                                                routingKey: _queueName,
                                                mandatory: false,
                                                basicProperties: properties,
                                                body: body);

                Console.WriteLine($"Message sent: {message} with retryCount: {0}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            throw; // Re-throw the exception for handling by the caller
        }
    }

    public async Task SendNotify(Request request)
    {
        string _hostName = "localhost"; // or the RabbitMQ server address
        string _queueName = "queuename_sendnotification";
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

                // Serialize the request object to JSON
                var message = JsonSerializer.Serialize(request);
                var body = Encoding.UTF8.GetBytes(message);

                // Create basic properties and add retryCount header
                var properties = new BasicProperties();
                properties.Headers = new Dictionary<string, object>
                {
                    { "retryCount", 0 }
                };

                // Publish the message
                await channel.BasicPublishAsync(exchange: "",
                                                routingKey: _queueName,
                                                mandatory: false,
                                                basicProperties: properties,
                                                body: body);

                Console.WriteLine($"Notification sent: {message} with retryCount: {0}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending notification: {ex.Message}");
            throw; // Re-throw the exception for handling by the caller
        }
    }
}
