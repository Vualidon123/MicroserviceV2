using ProductService.Models;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using ProductService.Datas;

namespace ProductService.Consumers
{
    public interface IOrderConsumer
    {
        public   Task UpdateProductQuantity();
    }
    public class OrderConsumer : IOrderConsumer
    {
        
        private readonly Repository<Product> _productRepository;

        public OrderConsumer(Repository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task UpdateProductQuantity()
        {
            string _hostName = "localhost"; // or the RabbitMQ server address
            string _queueName = "productRemain";

            var factory = new ConnectionFactory() { HostName = _hostName };
            using (var connection = await factory.CreateConnectionAsync())
            using (var channel = await connection.CreateChannelAsync())
            {
                // Declare the queue
                await channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    int retryCount = ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.ContainsKey("retryCount") ? (int)ea.BasicProperties.Headers["retryCount"] : 0;
                    
                    // Validate JSON format and deserialize
                    List<OrderDetailRequest>? orderDetails = null;
                    orderDetails = JsonSerializer.Deserialize<List<OrderDetailRequest>>(message);
                    if (orderDetails != null)
                    {
                        try
                        {
                            foreach (var order in orderDetails)
                            {
                                var product = await _productRepository.GetByIdAsync(order.ProductId);//Tim san pham dua tren don hang
                                product.Quantity -= order.Quantity;
                                await _productRepository.UpdateAsync(order.ProductId, product); //Cap nhat sl san pham
                            }
                            await channel.BasicAckAsync(ea.DeliveryTag, false);//Xac nhan xu ly thanh cong
                            Console.WriteLine($"Dong hang: {message} dat thanh cong");
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
                await Task.Delay(1000); // Adjust the delay as needed
            }
        }
    }
}