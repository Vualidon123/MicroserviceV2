using Application.Proxy;
using AuthenService.Request;
using JWT_Authen.Proxy.Web.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMqGateWay.Controllers;

namespace Start.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiGateWay : ControllerBase
    {     
        private readonly ILogger<ApiGateWay> _logger;
        private readonly IHttpEmpProxy _httpEmpProxy;
        private readonly IHttpAuthenProxy _httpAuthenProxy;
        private readonly IHttpRabbitMqProxy _httpRabbitMqProxy;
        public ApiGateWay(ILogger<ApiGateWay> logger,IHttpEmpProxy httpEmpProxy,IHttpAuthenProxy httpAuthenProxy,IHttpRabbitMqProxy httpRabbitMqProxy)
        {
            _logger = logger;
            _httpEmpProxy = httpEmpProxy;
            _httpAuthenProxy = httpAuthenProxy;
            _httpRabbitMqProxy = httpRabbitMqProxy;
        }
        [Route("authen/Login")]
        [HttpPost]
        public async Task<IActionResult> ForwardPost_Login([FromBody] LoginReq user)
        {
            try
            {
                var (statusCode, content) = await _httpEmpProxy.AuthLogin(user);
                return StatusCode((int)statusCode, content);
            }
            catch (Exception ex)
            {

                _logger.LogError($"Error in ForwardPost: {ex.Message}");
                return StatusCode(500, $"Error processing request: {ex.Message}");
            }
        }
        [Route("emp/get")]
        [HttpGet]
        public async Task<IActionResult> ForwarGet_Emp()
        {
            try
            {
                /* string authorizationHeader = HttpContext.Request.Headers["Authorization"];
                 string token = authorizationHeader.Replace("Bearer","").Trim();*/
                var (statusCode, content) = await _httpAuthenProxy.EmpGets(/*token*/);
                return StatusCode((int)statusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForwardPost: {ex.Message}");
                return StatusCode(500, $"Error processing request: {ex.Message}");
            }
        }
        [Route("emp/delete")]
        [HttpDelete]
        public async Task<IActionResult> ForwardDelete_Emp(int id)
        {
            try
            {
                /*string authorizationHeader = HttpContext.Request.Headers["Authorization"];
                string token = authorizationHeader.Replace("Bearer","").Trim();*/
                var (statusCode, content) = await _httpAuthenProxy.EmpDelete(id/*, token*/);
                return StatusCode((int)statusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForwardDelete: {ex.Message}");
                return StatusCode(500, $"Error processing request: {ex.Message}");
            }
        }
        [Route("rabbitMq/mail")]
        [HttpPost]
        public async Task<IActionResult> Post_Email(Request mail)
        {
            try
            {
                /*string authorizationHeader = HttpContext.Request.Headers["Authorization"];
                string token = authorizationHeader.Replace("Bearer","").Trim();*/
                var (statusCode, content) = await _httpRabbitMqProxy.Send_Email(mail/*, token*/);
                return StatusCode((int)statusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForwardDelete: {ex.Message}");
                return StatusCode(500, $"Error processing request: {ex.Message}");
            }
        }
        [Route("rabbitMq/noti")]
        [HttpPost]
        public async Task<IActionResult> Post_Noti(Request noti)
        {
            try
            {
                /*string authorizationHeader = HttpContext.Request.Headers["Authorization"];
                string token = authorizationHeader.Replace("Bearer","").Trim();*/
                var (statusCode, content) = await _httpRabbitMqProxy.Send_Noti(noti);
                return StatusCode((int)statusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForwardDelete: {ex.Message}");
                return StatusCode(500, $"Error processing request: {ex.Message}");
            }
        }
    }
}
