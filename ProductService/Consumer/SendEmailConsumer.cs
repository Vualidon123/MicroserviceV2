using ProductService.Models;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using ProductService.Datas;

namespace ProductService.Services
{
    public interface IEmailConsumer
    {
        Task ConsumeMessageAsync();
        Task SendEmailAsync(OrderRequest request);
    }    
    public class EmailConsumer : IEmailConsumer
    {
        private readonly Repository<Product> _productRepository;
        public EmailConsumer(Repository<Product> productService)
        {
            _productRepository = productService;
        }

        public async Task ConsumeMessageAsync()      
        {
            string _hostName = "localhost"; // or the RabbitMQ server address
            string _queueName = "orderProduct";

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
                    OrderRequest? orderDetails = null;
                    orderDetails = JsonSerializer.Deserialize<OrderRequest> (message);
                    if (orderDetails != null)
                    {
                        try
                        {   

                            await channel.BasicAckAsync(ea.DeliveryTag, false);//Xac nhan xu ly thanh cong
                            await SendEmailAsync(orderDetails);
                            Console.WriteLine($"Dang gui email voi noi dung: {message}");
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

        public async Task SendEmailAsync(OrderRequest email)
        {
            var fromEmail = "lacduy5@gmail.com";
            var fromPassword = "fbaj kqfc srkp wvmp"; // Use an App Password if 2-Step Verification is enabled

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Order Bill",
                Body = await GenerateEmailBodyAsync(email),
                IsBodyHtml = true,
            };
            mailMessage.To.Add("lacduy5@gmail.com");
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"SMTP Exception: {ex.Message}");
                throw;
            }
        }
        private async Task<string> GenerateEmailBodyAsync(OrderRequest email)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<h1>Order Details</h1>");
            sb.AppendLine($"<p><strong>Order Date:</strong> {email.OrderDate:yyyy-MM-dd}</p>");
            sb.AppendLine($"<p><strong>Customer ID:</strong> {email.CusId}</p>");

            sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr style='background-color: #f2f2f2;'>");
            sb.AppendLine("<th>Product ID</th>");
            sb.AppendLine("<th>Product Name</th>");
            sb.AppendLine("<th>Quantity</th>");
            sb.AppendLine("<th>Unit Price</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var detail in email.OrderDetails)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                string productName = product?.ProductName ?? "Unknown";
                string price = product?.Price.ToString("C2") ?? "N/A"; // Formats as currency

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{detail.ProductId}</td>");
                sb.AppendLine($"<td>{productName}</td>");
                sb.AppendLine($"<td>{detail.Quantity}</td>");
                sb.AppendLine($"<td>{price}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }
    }
}