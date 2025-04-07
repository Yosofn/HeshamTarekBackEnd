using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public class TokenService:ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public TokenService(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // دالة لإنشاء Access Token
        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.SchoolName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
             new Claim("UserType", user.UserType.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenLifespanMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // دالة لإنشاء Refresh Token
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        // دالة للتحقق من صلاحية Refresh Token
        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {  
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
        }

        // دالة لتجديد Access Token باستخدام Refresh Token
        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            var tokenEntry = await GetRefreshTokenAsync(refreshToken);

            if (tokenEntry == null || tokenEntry.Expiration < DateTime.UtcNow)
            {
                return null; // توكن غير صالح أو انتهت صلاحيته
            }

            var user = await _context.Users.FindAsync(tokenEntry.UserId);
            if (user == null)
            {
                return null;
            }

            return GenerateAccessToken(user); // إنشاء توكن جديد
        }

       
    }

}
