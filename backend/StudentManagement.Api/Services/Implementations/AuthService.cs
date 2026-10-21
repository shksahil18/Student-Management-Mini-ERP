using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.Api.Data;
using StudentManagement.Api.DTOs.Auth;
using StudentManagement.Api.DTOs.Common;
using StudentManagement.Api.Entities;
using StudentManagement.Api.Services.Interfaces;

namespace StudentManagement.Api.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        IOptions<EmailOptions> emailOptions,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower());

        if (user == null || !user.IsActive)
        {
            return null;
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return null;
        }

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<ApiResponseDto<UserDto>> RegisterAsync(RegisterRequestDto request)
    {
        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLower();

        // 1. Check duplicate username (case-insensitive)
        var existingUsername = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == normalizedUsername.ToLower());
        if (existingUsername)
        {
            return ApiResponseDto<UserDto>.Fail("A user with this username already exists.");
        }

        // 2. Check duplicate email (case-insensitive)
        var existingEmail = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (existingEmail)
        {
            return ApiResponseDto<UserDto>.Fail("A user with this email address already exists.");
        }

        // 3. Create new user - strictly force Role = "Staff" (never Admin)
        var newUser = new Entities.User
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Staff",
            IsActive = true,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        // 4. Create single-use Email Verification Token
        var rawToken = GenerateSecureRandomToken();
        var tokenHash = HashToken(rawToken);

        var verificationToken = new EmailVerificationToken
        {
            UserId = newUser.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(_emailOptions.VerificationTokenExpiryHours),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.EmailVerificationTokens.AddAsync(verificationToken);
        await _context.SaveChangesAsync();

        // 5. Send Verification Email via Outbox
        var verificationUrl = $"{_emailOptions.FrontendBaseUrl}/verify-email?token={rawToken}";
        var emailMessage = _emailTemplateService.BuildEmailVerificationEmail(newUser.Email, newUser.Username, verificationUrl);
        await _emailService.QueueEmailAsync(emailMessage);

        var userDto = new UserDto
        {
            Id = newUser.Id,
            Username = newUser.Username,
            Email = newUser.Email,
            Role = newUser.Role
        };

        return ApiResponseDto<UserDto>.Ok(userDto, "Account registered successfully. A verification link has been sent to your email.");
    }

    public async Task<ApiResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        // Security rule: Always return generic message to avoid email enumeration
        if (user == null || !user.IsActive)
        {
            _logger.LogInformation("Password reset requested for non-existent or inactive email: {Email}", normalizedEmail);
            return ApiResponseDto<bool>.Ok(true, "If an account exists for this email, password reset instructions will be sent.");
        }

        // Invalidate any prior unused reset tokens for this user
        var activeTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var t in activeTokens)
        {
            t.IsUsed = true;
            t.UsedAt = DateTime.UtcNow;
        }

        // Generate cryptographically secure token & hash
        var rawToken = GenerateSecureRandomToken();
        var tokenHash = HashToken(rawToken);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_emailOptions.PasswordResetTokenExpiryMinutes),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.PasswordResetTokens.AddAsync(resetToken);
        await _context.SaveChangesAsync();

        // Send Reset Email
        var resetUrl = $"{_emailOptions.FrontendBaseUrl}/reset-password?token={rawToken}";
        var emailMessage = _emailTemplateService.BuildPasswordResetEmail(user.Email, user.Username, resetUrl);
        await _emailService.QueueEmailAsync(emailMessage);

        return ApiResponseDto<bool>.Ok(true, "If an account exists for this email, password reset instructions will be sent.");
    }

    public async Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return ApiResponseDto<bool>.Fail("Verification token is missing.");
        }

        var tokenHash = HashToken(request.Token.Trim());
        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt <= DateTime.UtcNow)
        {
            return ApiResponseDto<bool>.Fail("This password reset link is invalid, expired, or has already been used.");
        }

        var user = resetToken.User;
        if (user == null || !user.IsActive)
        {
            return ApiResponseDto<bool>.Fail("The account associated with this token is inactive or not found.");
        }

        // Update password using BCrypt
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        
        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;

        // Invalidate any other active reset tokens for this user
        var otherTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync();
        foreach (var ot in otherTokens)
        {
            ot.IsUsed = true;
            ot.UsedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Password successfully reset for user '{Username}'", user.Username);

        return ApiResponseDto<bool>.Ok(true, "Your password has been successfully reset. You can now sign in with your new password.");
    }

    public async Task<ApiResponseDto<bool>> VerifyEmailAsync(VerifyEmailRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return ApiResponseDto<bool>.Fail("Verification token is missing.");
        }

        var tokenHash = HashToken(request.Token.Trim());
        var verificationToken = await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (verificationToken == null || verificationToken.IsUsed || verificationToken.ExpiresAt <= DateTime.UtcNow)
        {
            return ApiResponseDto<bool>.Fail("This email verification link is invalid, expired, or has already been used.");
        }

        var user = verificationToken.User;
        user.IsEmailVerified = true;

        verificationToken.IsUsed = true;
        verificationToken.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Email verified for user '{Username}'", user.Username);

        return ApiResponseDto<bool>.Ok(true, "Your email has been verified successfully. Welcome to the portal!");
    }

    public async Task<ApiResponseDto<bool>> ResendVerificationAsync(ResendVerificationRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        // Security rule: Always return success message
        if (user == null || !user.IsActive || user.IsEmailVerified)
        {
            return ApiResponseDto<bool>.Ok(true, "If an unverified account exists for this email, a new verification link has been sent.");
        }

        // Invalidate previous unexpired verification tokens
        var activeTokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
        foreach (var t in activeTokens)
        {
            t.IsUsed = true;
            t.UsedAt = DateTime.UtcNow;
        }

        var rawToken = GenerateSecureRandomToken();
        var tokenHash = HashToken(rawToken);

        var verificationToken = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(_emailOptions.VerificationTokenExpiryHours),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _context.EmailVerificationTokens.AddAsync(verificationToken);
        await _context.SaveChangesAsync();

        var verificationUrl = $"{_emailOptions.FrontendBaseUrl}/verify-email?token={rawToken}";
        var emailMessage = _emailTemplateService.BuildEmailVerificationEmail(user.Email, user.Username, verificationUrl);
        await _emailService.QueueEmailAsync(emailMessage);

        return ApiResponseDto<bool>.Ok(true, "If an unverified account exists for this email, a new verification link has been sent.");
    }

    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToHexString(randomBytes).ToLowerInvariant();
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string GenerateJwtToken(Entities.User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? "StudentManagementMiniErpSuperSecretSecurityKey2026!";
        var issuer = jwtSettings["Issuer"] ?? "StudentManagementApi";
        var audience = jwtSettings["Audience"] ?? "StudentManagementClient";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "480");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

