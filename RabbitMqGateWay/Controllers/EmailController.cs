using EmpService.Consumer;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RabbitMqGateWay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly RabbitMqProducerService _rabbitMqProducerService;
        private readonly IRabbitMqConsumer _rabbitMqConsumer;
        private readonly RabitIRepository<Email> _emailRepository;
        private readonly RabitIRepository<Notification> _notifyRepository;

        public EmailController(RabbitMqProducerService rabbitMqProducerService, IRabbitMqConsumer rabbitMqConsumer, RabitIRepository<Email> emailRepository, RabitIRepository<Notification> notifyRepository)
        {
            _rabbitMqProducerService = rabbitMqProducerService;
            _rabbitMqConsumer = rabbitMqConsumer;
            _emailRepository = emailRepository;
            _notifyRepository = notifyRepository;
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] Request emailRequest)
        {
            if (emailRequest == null || string.IsNullOrEmpty(emailRequest.To) || string.IsNullOrEmpty(emailRequest.Title) || string.IsNullOrEmpty(emailRequest.Body))
            {
                return BadRequest("Invalid email request.");
            }

            await _rabbitMqProducerService.SendMessageAsync(emailRequest);

            if (!await _rabbitMqConsumer.ConsumeMessageAsync())
            {
                return BadRequest("Email message sent to RabbitMQ, but there was an error consuming the message.");
            }
            var email = new Email   
            {
                SentDate = DateTime.UtcNow.AddHours(7),
                Subject = emailRequest.Title,
                Body = emailRequest.Body,
                emailrecive = emailRequest.To,
            };
            await _emailRepository.AddAsync(email);
            await _emailRepository.SaveChangesAsync();
            return Ok("Email message sent to RabbitMQ.");
        }

        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotification([FromBody] Request notificationRequest)
        {
            if (notificationRequest == null || string.IsNullOrEmpty(notificationRequest.To) || string.IsNullOrEmpty(notificationRequest.Title) || string.IsNullOrEmpty(notificationRequest.Body))
            {
                return BadRequest("Invalid notification request.");
            }

            await _rabbitMqProducerService.SendNotify(notificationRequest);

            if (!await _rabbitMqConsumer.ConsumeMessageAsync())
            {
                return BadRequest("Email message sent to RabbitMQ, but there was an error consuming the message.");
            }
            var noti = new Notification
            {
                CreatedDate = DateTime.UtcNow.AddHours(7),
                Message = notificationRequest.Body,
                emailrecive = notificationRequest.To,
                Title = notificationRequest.Title
            };
            await _notifyRepository.AddAsync(noti);
            await _notifyRepository.SaveChangesAsync();
            return Ok("Notification message sent to RabbitMQ.");
        }
    }

    public class Request
    {
        public string To { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}