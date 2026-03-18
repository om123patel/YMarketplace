using Identity.Domain.Entities;

namespace Identity.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        DateTime GetRefreshTokenExpiry();
        DateTime GetAccessTokenExpiry();
    }

}
