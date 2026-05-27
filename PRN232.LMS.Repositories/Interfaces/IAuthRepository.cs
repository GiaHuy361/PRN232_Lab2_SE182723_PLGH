using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<RefreshToken?> GetRefreshTokenByHashedTokenAsync(string hashedToken);
    Task AddRefreshTokenAsync(RefreshToken token);
    void UpdateRefreshToken(RefreshToken token);
    Task<int> SaveChangesAsync();
}
