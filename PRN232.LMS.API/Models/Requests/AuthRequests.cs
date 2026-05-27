using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "RefreshToken is required.")]
    [StringLength(500, ErrorMessage = "RefreshToken is invalid.")]
    public string RefreshToken { get; set; } = string.Empty;
}
