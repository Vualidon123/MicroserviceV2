
using System.Net;

namespace JWT_Authen.Proxy.Web.cs
{
    public interface IHttpAuthenProxy
    {
        /*Task<(HttpStatusCode statusCode, string content)> AuthLogin(LoginReq loginDto);*/
        Task<(HttpStatusCode statusCode, string content)> EmpGets();
        /* Task<(HttpStatusCode statusCode, string content)> EmpGets(userReq user);
         Task<(HttpStatusCode statusCode, string content)> EmpUpdate(int id, userReq user);*/
        Task<(HttpStatusCode statusCode, string content)> EmpDelete(int id/*, string token*/);
    }
    public class HttpAuthenProxy : IHttpAuthenProxy
    {
        private HttpClient _httpClient;


        public HttpAuthenProxy() // Constructor injection
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5211/");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

        }

        public async Task<(HttpStatusCode statusCode, string content)> EmpGets()
        {/*
            if (!await _functionAuthorizationService.CheckAuthorization(token, "READ")) // Replace with actual function code
            {
                return (HttpStatusCode.Forbidden, "Access denied"); // User is not authorized
            }*/
            var result = await _httpClient.GetAsync(_httpClient.BaseAddress + "api/Emp");
            if (result.IsSuccessStatusCode)
            {
                var responseContent = await result.Content.ReadAsStringAsync();
                return (HttpStatusCode.OK, responseContent); // Return the status code and response content
            }
            return (HttpStatusCode.BadRequest, "Bad Request"); // Return status code and message
        }
        /* public async Task<(HttpStatusCode statusCode, string content)> EmpGets(userReq user)
         {
             *//* if (!await _functionAuthorizationService.CheckAuthorization(token, "CREATE")) // Replace with actual function code
              {
                  return (HttpStatusCode.Forbidden, "Access denied"); // User is not authorized
              }*//*
             var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
             var result = await _httpClient.PostAsync(_httpClient.BaseAddress + "api/Emp", content);

             if (result.IsSuccessStatusCode)
             {
                 var responseContent = await result.Content.ReadAsStringAsync();
                 return (HttpStatusCode.OK, responseContent); // Return the status code and response content
             }
             return (HttpStatusCode.BadRequest, "Bad Request"); // Return status code and message
         }
         public async Task<(HttpStatusCode statusCode, string content)> EmpUpdate(int id, userReq user)
         {
             *//* if (!await _functionAuthorizationService.CheckAuthorization(token, "UPDATE")) // Replace with actual function code
              {
                  return (HttpStatusCode.Forbidden, "Access denied"); // User is not authorized
              }*//*
             var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
             var result = await _httpClient.PutAsync(_httpClient.BaseAddress + "api/Emp?id=" + id, content);

             if (result.IsSuccessStatusCode)
             {
                 var responseContent = await result.Content.ReadAsStringAsync();
                 return (HttpStatusCode.OK, responseContent); // Return the status code and response content
             }
             return (HttpStatusCode.BadRequest, "Bad Request"); // Return status code and message
         }*/
        public async Task<(HttpStatusCode statusCode, string content)> EmpDelete(int id)
        {
            /* if (!await _functionAuthorizationService.CheckAuthorization(token, "DELETE")) // Replace with actual function code
             {
                 return (HttpStatusCode.Forbidden, "Access denied"); // User is not authorized
             }*/
            var result = await _httpClient.DeleteAsync(_httpClient.BaseAddress + "api/Emp?id=" + id);

            if (result.IsSuccessStatusCode)
            {
                var responseContent = await result.Content.ReadAsStringAsync();
                return (HttpStatusCode.OK, responseContent); // Return the status code and response content
            }
            return (HttpStatusCode.BadRequest, "Bad Request"); // Return status code and message
        }
      


    }
}
