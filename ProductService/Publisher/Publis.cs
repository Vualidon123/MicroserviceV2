using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using ProductService.Models;
using ProductService.Datas;

namespace ProductService.Services
{
    public class Publis 
    {   
        private readonly Repository<Product> _productRepository;
        private readonly MongoDbContext _context;

        public Publis(MongoDbContext context)
        {
            _context=context;
            _productRepository = new Repository<Product>(context);
        }
        public async Task SendMessageAsync(OrderRequest request)
        {
            string _hostName = "localhost"; // or the RabbitMQ server address
            string _queueName = "orderProduct";
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
                        var properties = new BasicProperties
                        {
                            Headers = new Dictionary<string, object> { { "retryCount", 0 } }
                        };

                        // Publish the message
                         await channel.BasicPublishAsync(exchange: "",
                                                    routingKey: _queueName,
                                                    mandatory: false,
                                                    basicProperties: properties,
                                                    body: body);               
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                throw; // Re-throw the exception for handling by the caller
            }
        }
        public async Task UpdateRequest(List<OrderDetailRequest>  orderDetails)
        {
           
            string _hostName = "localhost"; // or the RabbitMQ server address
            string _queueName = "productRemain";
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
                    var message = JsonSerializer.Serialize(orderDetails);
                    var body = Encoding.UTF8.GetBytes(message);
                    // Create basic properties and add retryCount header
                    var properties = new BasicProperties
                    {
                        Headers = new Dictionary<string, object> { { "retryCount", 0 } }
                    };
                    properties.Persistent = true;
                    await channel.BasicPublishAsync(exchange: "",
                                                    routingKey: _queueName,
                                                    mandatory: false,
                                                    basicProperties: properties,
                                                    body: body);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                throw; // Re-throw the exception for handling by the caller
            }
        }
    }

}
