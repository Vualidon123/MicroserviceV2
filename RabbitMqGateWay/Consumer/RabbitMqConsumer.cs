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

            try
            {
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

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        if (string.IsNullOrWhiteSpace(message))
                        {
                            Console.WriteLine("Received an empty message.");
                            check = false;
                            return;
                        }

                        // Validate JSON format and deserialize
                        Request? emailRequest = null;
                        try
                        {
                            emailRequest = JsonSerializer.Deserialize<Request>(message);
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine($"JSON Exception: {jsonEx.Message}");
                            check = false;
                            return;
                        }

                        if (emailRequest != null)
                        {
                            try
                            {
                                await SendEmailAsync(emailRequest);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error sending email: {ex.Message}");
                                check = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to deserialize email request: Message might be improperly formatted.");
                            check = false;
                        }
                    };

                    await channel.BasicConsumeAsync(queue: _queueName,
                                                    autoAck: true,
                                                    consumer: consumer);

                    // Wait for the consumer to process messages
                    await Task.Delay(1000); // Adjust the delay as needed
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error consuming message: {ex.Message}");
                check = false;
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
