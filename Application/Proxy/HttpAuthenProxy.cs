using AuthenService.Request;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.Proxy
{
    public interface IHttpEmpProxy
    {
        Task<(HttpStatusCode statusCode, string content)> AuthLogin(LoginReq loginDto);
    }
    public class HttpEmpProxy : IHttpEmpProxy
    {
        private HttpClient _httpClient;

        public HttpEmpProxy() // Constructor injection
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5210/");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        public async Task<(HttpStatusCode statusCode, string content)> AuthLogin(LoginReq loginDto)
        {
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("api/Login", content);
            if (result.IsSuccessStatusCode)
            {
                var responseContent = await result.Content.ReadAsStringAsync();
                return (result.StatusCode, responseContent); // Return the status code and response content
            }
            return (result.StatusCode, await result.Content.ReadAsStringAsync()); // Return status code and message
        }
    }
}