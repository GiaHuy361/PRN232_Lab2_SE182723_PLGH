using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IAuthService
{
    Task<TokenResultModel?> LoginAsync(string username, string password);
    Task<TokenResultModel?> RefreshTokenAsync(string refreshToken);
}
