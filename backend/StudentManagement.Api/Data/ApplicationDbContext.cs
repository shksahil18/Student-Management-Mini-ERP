using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Entities;

namespace StudentManagement.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Fee> Fees => Set<Fee>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<EmailOutbox> EmailOutbox => Set<EmailOutbox>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<FeeReminderLog> FeeReminderLogs => Set<FeeReminderLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20).HasDefaultValue("Staff");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // 2. Student Configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentCode).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.StudentCode).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Course).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            // Composite search & filter indexes
            entity.HasIndex(e => new { e.LastName, e.FirstName });
            entity.HasIndex(e => new { e.Course, e.IsActive });
        });

        // 3. Fee Configuration
        modelBuilder.Entity<Fee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2).HasDefaultValue(0.00m);
            entity.Property(e => e.OutstandingAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            // Relationship: Student -> Fees (One-to-Many, Restrict Delete)
            entity.HasOne(f => f.Student)
                  .WithMany(s => s.Fees)
                  .HasForeignKey(f => f.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.Status);
        });

        // 4. Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiptNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.ReceiptNumber).IsUnique();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            // Relationship: Fee -> Payments (One-to-Many, Restrict Delete)
            entity.HasOne(p => p.Fee)
                  .WithMany(f => f.Payments)
                  .HasForeignKey(p => p.FeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relationship: User -> RecordedPayments (One-to-Many, Restrict Delete)
            entity.HasOne(p => p.CreatedByUser)
                  .WithMany(u => u.RecordedPayments)
                  .HasForeignKey(p => p.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.FeeId);
        });

        // 5. EmailOutbox Configuration
        modelBuilder.Entity<EmailOutbox>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ToEmail).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ToName).HasMaxLength(100);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(250);
            entity.Property(e => e.EmailType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReferenceId).HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => new { e.Status, e.RetryCount });
            entity.HasIndex(e => new { e.EmailType, e.ReferenceId });
        });

        // 6. EmailVerificationToken Configuration
        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(t => t.User)
                  .WithMany(u => u.VerificationTokens)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TokenHash);
            entity.HasIndex(e => new { e.UserId, e.IsUsed });
        });

        // 7. PasswordResetToken Configuration
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(t => t.User)
                  .WithMany(u => u.PasswordResetTokens)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TokenHash);
            entity.HasIndex(e => new { e.UserId, e.IsUsed });
        });

        // 8. FeeReminderLog Configuration
        modelBuilder.Entity<FeeReminderLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(150);
            entity.Property(e => e.OutstandingAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.SentAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(r => r.Fee)
                  .WithMany()
                  .HasForeignKey(r => r.FeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Student)
                  .WithMany()
                  .HasForeignKey(r => r.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.FeeId, e.ReminderType, e.SentAt });
        });
    }
}
