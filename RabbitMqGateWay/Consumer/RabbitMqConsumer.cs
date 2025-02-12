using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using RabbitMqGateWay.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EmpService.Consumer
{
    public interface IRabbitMqConsumer
    {
        Task<bool> ConsumeMessageAsync();
        Task SendEmailAsync(Request email);
        Task LogErrorToDatabaseAsync(string errorMessage);
    }
    public class RabbitMqConsumer : IRabbitMqConsumer
    {
        public async Task<bool> ConsumeMessageAsync()
        {
            string _hostName = "localhost"; // or the RabbitMQ server address
            string _queueName = "queuename_sendemail";

            var factory = new ConnectionFactory() { HostName = _hostName };
            var check = true;

          
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
                        int retryCount = ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.ContainsKey("retryCount") ?(int)ea.BasicProperties.Headers["retryCount"] : 0;
                        // Validate JSON format and deserialize
                        Request? emailRequest = null;
                        emailRequest = JsonSerializer.Deserialize<Request>(message);
                      
                        if (emailRequest != null)
                        {
                            try
                            {                              
                                await SendEmailAsync(emailRequest);
                               /* await channel.BasicAckAsync(ea.DeliveryTag, false);*/
                                Console.WriteLine($"✅ Email gửi thành công: {message}");
                            }
                            catch (Exception ex)
                            {                           
                                retryCount++;
                                if (retryCount>3)
                                {
                                    Console.WriteLine($"❌ Đưa vào DLQ: {message}");
                                    check = false;
                                    await channel.BasicNackAsync(ea.DeliveryTag, false, false); // Gửi vào Dead Letter Queue (không requeue)
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Lỗi gửi email ({retryCount}/{3}): {ex.Message}. Thử lại...");                                
                                    var properties = new BasicProperties();
                                    properties.Persistent = true;
                                    properties.Headers = new Dictionary<string, object> { { "retryCount", retryCount } };
                                    await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: false, basicProperties: properties, body: body);                     
                                    /*await channel.BasicAckAsync(ea.DeliveryTag, false);*/
                                }
                              
                            }
                        }
                    };
                await channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);
                // Wait for the consumer to process messages
                await Task.Delay(1000); // Adjust the delay as needed
                }           
            return check;
        }
        public async Task SendEmailAsync(Request email)
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
                Subject = email.Title,
                Body = email.Body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email.To);

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

        public async Task LogErrorToDatabaseAsync(string errorMessage)
        {
            // Implement the logic to log the error message to the database
            // This is a placeholder implementation
            Console.WriteLine($"Logging error to database: {errorMessage}");
            await Task.CompletedTask;
        }
    }
}
