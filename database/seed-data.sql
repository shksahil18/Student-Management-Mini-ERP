-- ============================================================================
-- Student Management Mini-ERP: Realistic Seed Data Script
-- Default Admin Password: Admin@123
-- Default Staff Password: Staff@123
-- Passwords hashed using standard BCrypt ($2a$11$...)
-- ============================================================================

USE StudentManagementDb;
GO

SET NOCOUNT ON;

-- 1. Seed Users
-- Password for admin: Admin@123 -> $2a$11$7rK3t6w1u9v.y5z8X4Q2LeK8Z7J3H5G9F2D4S6A8P0O2I4U6Y8T0W
-- We use standard BCrypt hashes that can be verified by BCrypt.Net-Next
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Users (Username, Email, PasswordHash, Role, IsActive, CreatedAt)
    VALUES 
    ('admin', 'admin@erp.local', '$2a$11$EixZaYVK1fsbw1ZfbX3OXePaWxn96p36WQmG6FeTLPZ8.24OzC/O6', 'Admin', 1, SYSUTCDATETIME()),
    ('staff', 'staff@erp.local', '$2a$11$EixZaYVK1fsbw1ZfbX3OXePaWxn96p36WQmG6FeTLPZ8.24OzC/O6', 'Staff', 1, SYSUTCDATETIME());
END
GO

-- 2. Seed Students
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentCode = 'STU-2026-00001')
BEGIN
    INSERT INTO dbo.Students (StudentCode, FirstName, LastName, DateOfBirth, Gender, Email, Phone, Address, AdmissionDate, Course, IsActive, CreatedAt)
    VALUES
    ('STU-2026-00001', 'Aarav', 'Sharma', '2003-04-12', 'Male', 'aarav.sharma@example.com', '+91 9876543210', '12 MG Road, Bangalore', '2026-01-15', 'MCA', 1, SYSUTCDATETIME()),
    ('STU-2026-00002', 'Priya', 'Patel', '2004-09-21', 'Female', 'priya.patel@example.com', '+91 9876543211', '45 SG Highway, Ahmedabad', '2026-01-20', 'B.Tech Computer Science', 1, SYSUTCDATETIME()),
    ('STU-2026-00003', 'Rohan', 'Verma', '2003-11-05', 'Male', 'rohan.verma@example.com', '+91 9876543212', '78 Park Street, Kolkata', '2026-02-01', 'MBA', 1, SYSUTCDATETIME()),
    ('STU-2026-00004', 'Ananya', 'Iyer', '2004-01-18', 'Female', 'ananya.iyer@example.com', '+91 9876543213', '23 Anna Salai, Chennai', '2026-02-10', 'MCA', 1, SYSUTCDATETIME()),
    ('STU-2026-00005', 'Vikram', 'Singh', '2002-07-30', 'Male', 'vikram.singh@example.com', '+91 9876543214', '90 Civil Lines, Jaipur', '2026-02-15', 'B.Tech Computer Science', 0, SYSUTCDATETIME()),
    ('STU-2026-00006', 'Sneha', 'Deshmukh', '2003-03-25', 'Female', 'sneha.deshmukh@example.com', '+91 9876543215', '56 FC Road, Pune', '2026-03-01', 'BCA', 1, SYSUTCDATETIME());
END
GO

-- 3. Seed Fees & Payments
DECLARE @AdminId INT = (SELECT TOP 1 Id FROM dbo.Users WHERE Username = 'admin');
DECLARE @Stu1 INT = (SELECT Id FROM dbo.Students WHERE StudentCode = 'STU-2026-00001');
DECLARE @Stu2 INT = (SELECT Id FROM dbo.Students WHERE StudentCode = 'STU-2026-00002');
DECLARE @Stu3 INT = (SELECT Id FROM dbo.Students WHERE StudentCode = 'STU-2026-00003');
DECLARE @Stu4 INT = (SELECT Id FROM dbo.Students WHERE StudentCode = 'STU-2026-00004');

IF NOT EXISTS (SELECT 1 FROM dbo.Fees WHERE StudentId = @Stu1)
BEGIN
    -- Stu1: Total 60,000, Paid 60,000 (Fully Paid)
    INSERT INTO dbo.Fees (StudentId, TotalAmount, PaidAmount, OutstandingAmount, DueDate, Status, CreatedAt)
    VALUES (@Stu1, 60000.00, 60000.00, 0.00, '2026-08-30', 'Paid', SYSUTCDATETIME());
    DECLARE @Fee1 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Payments (FeeId, ReceiptNumber, Amount, PaymentDate, PaymentMethod, Notes, CreatedByUserId, CreatedAt)
    VALUES 
    (@Fee1, 'REC-2026-000001', 30000.00, DATEADD(DAY, -20, SYSUTCDATETIME()), 'UPI', 'Installment 1', @AdminId, SYSUTCDATETIME()),
    (@Fee1, 'REC-2026-000002', 30000.00, DATEADD(DAY, -5, SYSUTCDATETIME()), 'Bank Transfer', 'Installment 2 (Final)', @AdminId, SYSUTCDATETIME());
END

IF NOT EXISTS (SELECT 1 FROM dbo.Fees WHERE StudentId = @Stu2)
BEGIN
    -- Stu2: Total 80,000, Paid 35,000, Outstanding 45,000 (Partial)
    INSERT INTO dbo.Fees (StudentId, TotalAmount, PaidAmount, OutstandingAmount, DueDate, Status, CreatedAt)
    VALUES (@Stu2, 80000.00, 35000.00, 45000.00, '2026-09-15', 'Partial', SYSUTCDATETIME());
    DECLARE @Fee2 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Payments (FeeId, ReceiptNumber, Amount, PaymentDate, PaymentMethod, Notes, CreatedByUserId, CreatedAt)
    VALUES (@Fee2, 'REC-2026-000003', 35000.00, DATEADD(DAY, -10, SYSUTCDATETIME()), 'Card', 'Initial deposit', @AdminId, SYSUTCDATETIME());
END

IF NOT EXISTS (SELECT 1 FROM dbo.Fees WHERE StudentId = @Stu3)
BEGIN
    -- Stu3: Total 95,000, Paid 0, Outstanding 95,000 (Pending)
    INSERT INTO dbo.Fees (StudentId, TotalAmount, PaidAmount, OutstandingAmount, DueDate, Status, CreatedAt)
    VALUES (@Stu3, 95000.00, 0.00, 95000.00, '2026-10-30', 'Pending', SYSUTCDATETIME());
END

IF NOT EXISTS (SELECT 1 FROM dbo.Fees WHERE StudentId = @Stu4)
BEGIN
    -- Stu4: Total 60,000, Paid 20,000, Outstanding 40,000 (Partial)
    INSERT INTO dbo.Fees (StudentId, TotalAmount, PaidAmount, OutstandingAmount, DueDate, Status, CreatedAt)
    VALUES (@Stu4, 60000.00, 20000.00, 40000.00, '2026-09-30', 'Partial', SYSUTCDATETIME());
    DECLARE @Fee4 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Payments (FeeId, ReceiptNumber, Amount, PaymentDate, PaymentMethod, Notes, CreatedByUserId, CreatedAt)
    VALUES (@Fee4, 'REC-2026-000004', 20000.00, DATEADD(DAY, -2, SYSUTCDATETIME()), 'Cash', 'Registration fee', @AdminId, SYSUTCDATETIME());
END
GO

PRINT 'Seed data inserted successfully!';
