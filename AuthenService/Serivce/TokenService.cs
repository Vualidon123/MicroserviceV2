using AuthenService.Request;
using Microsoft.Extensions.Options;

using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;


namespace AuthenService.Service
{
    public class TokenService
    {
        private readonly TokenRequest _tokenRequest;
        public TokenService(IOptions<TokenRequest> tokenRequest)
        {
            _tokenRequest = tokenRequest.Value;
        }
        public string GenerateToken(int empId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenRequest.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, empId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _tokenRequest.Issuer,
                audience: _tokenRequest.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_tokenRequest.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
