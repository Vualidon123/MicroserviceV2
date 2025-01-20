using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;

public class RabbitMqProducerService
{
    private readonly string _hostname;
    private readonly string _queueName;
    private IConnection _connection;

    public RabbitMqProducerService(IOptions<RabbitMqConfiguration> rabbitMqOptions)
    {
        _hostname = rabbitMqOptions.Value.HostName;
        _queueName = rabbitMqOptions.Value.QueueName;
        CreateConnection().GetAwaiter().GetResult();
    }

    private async Task CreateConnection()
    {
        var factory = new ConnectionFactory { HostName = _hostname };
        _connection = await factory.CreateConnectionAsync();
    }

    public async Task<bool> SendMessage<T>(T message)
    {
        using var channel = await _connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // Log the message
        Console.WriteLine($"Sending message: {json}");

        var basicProperties = new BasicProperties();
        var tcs = new TaskCompletionSource<bool>();

        channel.BasicReturnAsync += async (sender, args) =>
        {
            tcs.SetResult(false);
            await Task.CompletedTask;
        };

        await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: true, basicProperties: basicProperties, body: body);

        // Wait for a short period to see if the message is returned
        var result = await Task.WhenAny(tcs.Task, Task.Delay(1000));
        return result == tcs.Task && tcs.Task.Result;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
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
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmail);

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
}

public class RabbitMqConfiguration
{
    public string HostName { get; set; }
    public string QueueName { get; set; }
}
