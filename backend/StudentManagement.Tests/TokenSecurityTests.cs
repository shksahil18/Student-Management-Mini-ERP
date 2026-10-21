using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Data;
using StudentManagement.Api.Entities;
using Xunit;

namespace StudentManagement.Tests;

public class TokenSecurityTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public async Task EmailVerificationToken_ValidToken_MarksAsUsedAndVerifies()
    {
        using var db = CreateInMemoryDbContext();
        var rawToken = "sample_raw_verification_token_123456789";
        var tokenHash = HashToken(rawToken);

        var user = new User
        {
            Username = "teacher1",
            Email = "teacher1@example.com",
            PasswordHash = "hash",
            Role = "Staff",
            IsEmailVerified = false
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var verificationToken = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };
        db.EmailVerificationTokens.Add(verificationToken);
        await db.SaveChangesAsync();

        // Simulate token lookup and verification
        var foundToken = await db.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

        Assert.NotNull(foundToken);
        Assert.Equal(user.Id, foundToken.UserId);

        foundToken.IsUsed = true;
        foundToken.UsedAt = DateTime.UtcNow;
        foundToken.User.IsEmailVerified = true;
        await db.SaveChangesAsync();

        // Verify state is saved
        var reloadedUser = await db.Users.FindAsync(user.Id);
        Assert.True(reloadedUser!.IsEmailVerified);

        // Subsequent lookup should fail because IsUsed is true
        var reusedLookup = await db.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsUsed);
        Assert.Null(reusedLookup);
    }

    [Fact]
    public async Task PasswordResetToken_ExpiredToken_IsNotValid()
    {
        using var db = CreateInMemoryDbContext();
        var rawToken = "expired_token_abc";
        var tokenHash = HashToken(rawToken);

        var user = new User
        {
            Username = "staff2",
            Email = "staff2@example.com",
            PasswordHash = "hash",
            Role = "Staff",
            IsEmailVerified = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired 1 hour ago
            IsUsed = false
        };
        db.PasswordResetTokens.Add(resetToken);
        await db.SaveChangesAsync();

        // Query active tokens
        var activeToken = await db.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

        Assert.Null(activeToken);
    }

    [Fact]
    public void Sha256Hash_ProducesDeterministicLowerHexOutput()
    {
        var token = "MySecureToken123!";
        var hash1 = HashToken(token);
        var hash2 = HashToken(token);

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length); // 256 bits = 32 bytes = 64 hex characters
        Assert.True(hash1.All(c => "0123456789abcdef".Contains(c)));
    }
}

