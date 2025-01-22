using AuthenService.Request;
using AuthenService.Service;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenService.Serivce
{
    public class AuthorizeService
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redisService;
        public AuthorizeService(RequestDelegate next, IConfiguration configuration, RedisService redisService)
        {
            _next = next;
            _configuration = configuration;
            _redisService = redisService;
            
        }
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["JWTSettings:SecretKey"];
            var issuer = _configuration["JWTSettings:Issuer"];
            var audience = _configuration["JWTSettings:Audience"];

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience
            };

            try
            {
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null; // Token is invalid
            }
        }
        public async Task<bool> CheckAuthorization(string token, string functionCode)
        {
            var claimsPrincipal = ValidateToken(token);
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return false;
            }
            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return false; // User ID is not a valid integer
            }
            return await HasAccessAsync(userId, functionCode);
        }
        public async Task<bool> HasAccessAsync(int userId, string functionCode)
        {
            var empCache = await _redisService.GetAsync<CacheRequest>($"emp:{userId}");
            if (empCache == null) return false;
            return empCache.Functions
            .Any(f => f.Equals(functionCode, StringComparison.OrdinalIgnoreCase));
        }
        public async Task Invoke(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer", "").Trim();
            var claimsPrincipal = ValidateToken(token);
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || int.Parse(userIdClaim.Value) == 0)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
            // Determine function code based on the request path
            string functionCode = "READ";

            if (!await CheckAuthorization(token, functionCode))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden");
                return;
            }
            await _next(context);
        }
    }

}
