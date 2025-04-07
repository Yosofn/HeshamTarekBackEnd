using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public interface ITokenService
    {
        public string GenerateAccessToken(User user);
        public string GenerateRefreshToken();
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task<string> RefreshAccessTokenAsync(string refreshToken);
      


    }
}
