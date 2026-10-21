using System.Net;
using System.Text;
using StudentManagement.Api.DTOs.Fees;
using StudentManagement.Api.DTOs.Students;
using StudentManagement.Api.Entities;

namespace StudentManagement.Api.Services.Interfaces;

public interface IEmailTemplateService
{
    EmailMessageDto BuildAdmissionConfirmationEmail(StudentDto student);
    EmailMessageDto BuildFeeReceiptEmail(ReceiptDto receipt, string studentEmail);
    EmailMessageDto BuildUpcomingFeeReminderEmail(Student student, Fee fee, int daysLeft);
    EmailMessageDto BuildOverdueFeeReminderEmail(Student student, Fee fee, int daysOverdue);
    EmailMessageDto BuildEmailVerificationEmail(string toEmail, string username, string verificationLink);
    EmailMessageDto BuildPasswordResetEmail(string toEmail, string username, string resetLink);
}

