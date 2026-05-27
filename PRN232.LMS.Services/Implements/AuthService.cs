using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implements;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IAuthRepository authRepository, IConfiguration configuration)
    {
        _authRepository = authRepository;
        _configuration = configuration;
    }

    public async Task<TokenResultModel?> LoginAsync(string username, string password)
    {
        var user = await _authRepository.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return null;
        }

        // Verify password using BCrypt.Net
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return null;
        }

        return await GenerateTokensAndSaveAsync(user);
    }

    public async Task<TokenResultModel?> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        // Hash the refresh token using SHA-256
        string hashedToken = HashToken(refreshToken);

        // Find the token in the database
        var existingToken = await _authRepository.GetRefreshTokenByHashedTokenAsync(hashedToken);
        if (existingToken == null)
        {
            return null;
        }

        // Validate token status
        if (existingToken.IsUsed || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        // Mark the old token as used/revoked
        existingToken.IsUsed = true;
        existingToken.RevokedAt = DateTime.UtcNow;
        _authRepository.UpdateRefreshToken(existingToken);

        // Generate a new pair of tokens
        var user = existingToken.User;
        return await GenerateTokensAndSaveAsync(user);
    }

    private async Task<TokenResultModel> GenerateTokensAndSaveAsync(User user)
    {
        // 1. Generate JWT Access Token
        var (accessToken, expiresInSeconds) = GenerateJwtToken(user);

        // 2. Generate secure Random Refresh Token
        string rawRefreshToken = GenerateRandomTokenString();
        string hashedRefreshToken = HashToken(rawRefreshToken);

        // 3. Save Hashed Refresh Token to database
        int refreshTokenDays = GetConfigValue("Jwt:RefreshTokenDays", 7);
        var tokenEntity = new RefreshToken
        {
            UserId = user.UserId,
            Token = hashedRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
            IsUsed = false,
            IsRevoked = false
        };

        await _authRepository.AddRefreshTokenAsync(tokenEntity);
        await _authRepository.SaveChangesAsync();

        return new TokenResultModel
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresIn = expiresInSeconds,
            Username = user.Username,
            Role = user.Role
        };
    }

    private (string Token, int ExpiresInSeconds) GenerateJwtToken(User user)
    {
        string secretKey = _configuration["Jwt:Secret"] ?? _configuration["Jwt__Secret"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("JWT secret is not configured. Set environment variable Jwt__Secret.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        string issuer = _configuration["Jwt:Issuer"] ?? "PRN232.LMS.API";
        string audience = _configuration["Jwt:Audience"] ?? "PRN232.LMS.Client";
        int accessTokenMinutes = GetConfigValue("Jwt:AccessTokenMinutes", 60);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var expires = DateTime.UtcNow.AddMinutes(accessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        int expiresInSeconds = accessTokenMinutes * 60;

        return (tokenString, expiresInSeconds);
    }

    private string GenerateRandomTokenString()
    {
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    private string HashToken(string token)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    private int GetConfigValue(string key, int defaultValue)
    {
        string? valStr = _configuration[key];
        if (int.TryParse(valStr, out int result))
        {
            return result;
        }
        return defaultValue;
    }
}
