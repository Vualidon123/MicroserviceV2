using Microsoft.AspNetCore.Mvc;

namespace RabbitMqGateWay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly RabbitMqProducerService _rabbitMqProducerService;

        public EmailController(RabbitMqProducerService rabbitMqProducerService)
        {
            _rabbitMqProducerService = rabbitMqProducerService;
        }

        [HttpPost("send")]
        public IActionResult SendEmail([FromBody] EmailRequest emailRequest)
        {
            _rabbitMqProducerService.SendMessage(emailRequest);
            return Ok("Email request sent to RabbitMQ");
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmailAsync([FromBody] EmailRequest emailRequest)
        {
            await _rabbitMqProducerService.SendEmailAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);
            return Ok("Email sent successfully");
        }
    }

    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}