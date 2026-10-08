using Microsoft.EntityFrameworkCore;
using StudentManagement.Api.Entities;

namespace StudentManagement.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure database exists
        await context.Database.EnsureCreatedAsync();

        // 1. Seed or Update Users with fresh BCrypt hashes
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = "admin",
                Email = "admin@erp.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(adminUser);
        }
        else
        {
            adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        }

        var staffUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "staff");
        if (staffUser == null)
        {
            staffUser = new User
            {
                Username = "staff",
                Email = "staff@erp.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Role = "Staff",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(staffUser);
        }
        else
        {
            staffUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123");
        }

        await context.SaveChangesAsync();

        // 2. Seed Students if not present
        if (!await context.Students.AnyAsync())
        {
            var admin = await context.Users.FirstAsync(u => u.Username == "admin");

            var students = new List<Student>
            {
                new()
                {
                    StudentCode = "STU-2026-00001",
                    FirstName = "Aarav",
                    LastName = "Sharma",
                    DateOfBirth = new DateTime(2003, 4, 12),
                    Gender = "Male",
                    Email = "aarav.sharma@example.com",
                    Phone = "+91 9876543210",
                    Address = "12 MG Road, Bangalore",
                    AdmissionDate = new DateTime(2026, 1, 15),
                    Course = "MCA",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    StudentCode = "STU-2026-00002",
                    FirstName = "Priya",
                    LastName = "Patel",
                    DateOfBirth = new DateTime(2004, 9, 21),
                    Gender = "Female",
                    Email = "priya.patel@example.com",
                    Phone = "+91 9876543211",
                    Address = "45 SG Highway, Ahmedabad",
                    AdmissionDate = new DateTime(2026, 1, 20),
                    Course = "B.Tech Computer Science",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    StudentCode = "STU-2026-00003",
                    FirstName = "Rohan",
                    LastName = "Verma",
                    DateOfBirth = new DateTime(2003, 11, 5),
                    Gender = "Male",
                    Email = "rohan.verma@example.com",
                    Phone = "+91 9876543212",
                    Address = "78 Park Street, Kolkata",
                    AdmissionDate = new DateTime(2026, 2, 1),
                    Course = "MBA",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    StudentCode = "STU-2026-00004",
                    FirstName = "Ananya",
                    LastName = "Iyer",
                    DateOfBirth = new DateTime(2004, 1, 18),
                    Gender = "Female",
                    Email = "ananya.iyer@example.com",
                    Phone = "+91 9876543213",
                    Address = "23 Anna Salai, Chennai",
                    AdmissionDate = new DateTime(2026, 2, 10),
                    Course = "MCA",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    StudentCode = "STU-2026-00005",
                    FirstName = "Vikram",
                    LastName = "Singh",
                    DateOfBirth = new DateTime(2002, 7, 30),
                    Gender = "Male",
                    Email = "vikram.singh@example.com",
                    Phone = "+91 9876543214",
                    Address = "90 Civil Lines, Jaipur",
                    AdmissionDate = new DateTime(2026, 2, 15),
                    Course = "B.Tech Computer Science",
                    IsActive = false, // Inactive student demo
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    StudentCode = "STU-2026-00006",
                    FirstName = "Sneha",
                    LastName = "Deshmukh",
                    DateOfBirth = new DateTime(2003, 3, 25),
                    Gender = "Female",
                    Email = "sneha.deshmukh@example.com",
                    Phone = "+91 9876543215",
                    Address = "56 FC Road, Pune",
                    AdmissionDate = new DateTime(2026, 3, 1),
                    Course = "BCA",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Students.AddRangeAsync(students);
            await context.SaveChangesAsync();

            // 3. Seed Fees & Payments
            var stu1 = students[0];
            var stu2 = students[1];
            var stu3 = students[2];
            var stu4 = students[3];

            // Stu 1: Total 60,000, Fully Paid in 2 installments
            var fee1 = new Fee
            {
                StudentId = stu1.Id,
                TotalAmount = 60000.00m,
                PaidAmount = 60000.00m,
                OutstandingAmount = 0.00m,
                DueDate = DateTime.UtcNow.AddMonths(2),
                Status = "Paid",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            await context.Fees.AddAsync(fee1);
            await context.SaveChangesAsync();

            var p1 = new Payment
            {
                FeeId = fee1.Id,
                ReceiptNumber = "REC-2026-000001",
                Amount = 30000.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-20),
                PaymentMethod = "UPI",
                Notes = "Semester installment 1",
                CreatedByUserId = admin.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            };
            var p2 = new Payment
            {
                FeeId = fee1.Id,
                ReceiptNumber = "REC-2026-000002",
                Amount = 30000.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-5),
                PaymentMethod = "Bank Transfer",
                Notes = "Final settlement",
                CreatedByUserId = admin.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };
            await context.Payments.AddRangeAsync(p1, p2);

            // Stu 2: Total 80,000, Partial Paid 35,000
            var fee2 = new Fee
            {
                StudentId = stu2.Id,
                TotalAmount = 80000.00m,
                PaidAmount = 35000.00m,
                OutstandingAmount = 45000.00m,
                DueDate = DateTime.UtcNow.AddMonths(1),
                Status = "Partial",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };
            await context.Fees.AddAsync(fee2);
            await context.SaveChangesAsync();

            var p3 = new Payment
            {
                FeeId = fee2.Id,
                ReceiptNumber = "REC-2026-000003",
                Amount = 35000.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-10),
                PaymentMethod = "Card",
                Notes = "Initial admission deposit",
                CreatedByUserId = admin.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };
            await context.Payments.AddAsync(p3);

            // Stu 3: Total 95,000, 0 Paid, Fully Outstanding (Pending)
            var fee3 = new Fee
            {
                StudentId = stu3.Id,
                TotalAmount = 95000.00m,
                PaidAmount = 0.00m,
                OutstandingAmount = 95000.00m,
                DueDate = DateTime.UtcNow.AddDays(20),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };
            await context.Fees.AddAsync(fee3);

            // Stu 4: Total 60,000, 20,000 Paid (Partial)
            var fee4 = new Fee
            {
                StudentId = stu4.Id,
                TotalAmount = 60000.00m,
                PaidAmount = 20000.00m,
                OutstandingAmount = 40000.00m,
                DueDate = DateTime.UtcNow.AddDays(40),
                Status = "Partial",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };
            await context.Fees.AddAsync(fee4);
            await context.SaveChangesAsync();

            var p4 = new Payment
            {
                FeeId = fee4.Id,
                ReceiptNumber = "REC-2026-000004",
                Amount = 20000.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-2),
                PaymentMethod = "Cash",
                Notes = "Registration fee token",
                CreatedByUserId = admin.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };
            await context.Payments.AddAsync(p4);

            await context.SaveChangesAsync();
        }
    }
}
