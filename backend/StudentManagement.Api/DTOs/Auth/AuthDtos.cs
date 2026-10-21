namespace StudentManagement.Api.DTOs.Auth;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class RegisterRequestDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Username is required.")]
    [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string Username { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email address is required.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Password is required.")]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Password confirmation is required.")]
    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email address is required.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Verification token is required.")]
    public string Token { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "New password is required.")]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Password confirmation is required.")]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class VerifyEmailRequestDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Verification token is required.")]
    public string Token { get; set; } = string.Empty;
}

public class ResendVerificationRequestDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email address is required.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; set; } = string.Empty;
}
