using ExpenseVista.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseVista.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration configuration;

        public JwtService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GenerateAccessToken(ApplicationUser user, out DateTime expiresAt)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
            };

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var durationMinutes = double.Parse(jwtSettings["AcessTokenDurationInMinutes"] ?? "15");
            expiresAt = DateTime.UtcNow.AddMinutes(durationMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generates a secure random refresh token string (not the hash), and cleans up 
        public string GenerateRefreshTokenString(int size = 64)
        {
            var randomNumber = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Hash the refresh token for storage (SHA256)
        public string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hashed = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hashed);
        }

        // Validate token signature & return ClaimsPrincipal even if expired
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
                ValidateLifetime = false // allow expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                //Is this a JWT? (No? Return null). Is this using the HmacSha256 algorithm? (No ? Return null).
                if (securityToken is not JwtSecurityToken jwtSecurityToken
                    || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
