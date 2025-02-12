using ProductService.Models;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using ProductService.Datas;

namespace ProductService.Services
{
    public class ProductConsumer : BackgroundService
    {
        private readonly Repository<Product> _productRepository;

        public ProductConsumer(Repository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string _hostName = "localhost";
            string _queueName = "productRemain";

            var factory = new ConnectionFactory() { HostName = _hostName };
            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                await channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    int retryCount = ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.ContainsKey("retryCount") ? (int)ea.BasicProperties.Headers["retryCount"] : 0;
                    List<OrderDetailRequest>? orderDetails = JsonSerializer.Deserialize<List<OrderDetailRequest>>(message);
                    if (orderDetails != null)
                    {
                        try
                        {
                            foreach (var order in orderDetails)
                            {
                                var product = await _productRepository.GetByIdAsync(order.ProductId);
                                product.Quantity -= order.Quantity;
                                await _productRepository.UpdateAsync(order.ProductId, product);
                            }
                            await channel.BasicAckAsync(ea.DeliveryTag, false);
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            if (retryCount > 3)
                            {
                                Console.WriteLine($"❌ Đưa vào DLQ: {message}");
                                await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                            }
                            else
                            {
                                Console.WriteLine($" Lỗi  ({retryCount}/{3}): {ex.Message}. Thử lại...");
                                var properties = new BasicProperties();
                                properties.Persistent = true;
                                properties.Headers = new Dictionary<string, object> { { "retryCount", retryCount } };
                                await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: false, basicProperties: properties, body: body);
                                await channel.BasicAckAsync(ea.DeliveryTag, false);
                            }
                        }
                    }
                };
                await channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}