/*using RabbitMqGateWay.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Proxy
{
    public interface IHttpRabbitMqProxy
    {
        Task<(HttpStatusCode statusCode, string content)> Send_Email(Request mail);
        Task<(HttpStatusCode statusCode, string content)> Send_Noti(Request mail);
    }
    public class HttpRabbitMqProxy : IHttpRabbitMqProxy
    {
        private HttpClient _httpClient;
        public HttpRabbitMqProxy() 
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5212/");
            _httpClient.Timeout = TimeSpan.FromSeconds(630);
        }
        public async Task<(HttpStatusCode statusCode, string content)> Send_Email(Request mail)
        {
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(mail), Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("api/Email/send-email", content);
            if (result.IsSuccessStatusCode)
            {
                var responseContent = await result.Content.ReadAsStringAsync();
                return (result.StatusCode, responseContent); // Return the status code and response content
            }
            return (result.StatusCode, await result.Content.ReadAsStringAsync()); // Return status code and message
        }

        public async Task<(HttpStatusCode statusCode, string content)> Send_Noti(Request mail)
           {
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(mail), Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("api/Email/send-notification", content);
            if (result.IsSuccessStatusCode)
            {
                var responseContent = await result.Content.ReadAsStringAsync();
                return (result.StatusCode, responseContent); // Return the status code and response content
            }
            return (result.StatusCode, await result.Content.ReadAsStringAsync()); // Return status code and message
        }
    }
}
*/