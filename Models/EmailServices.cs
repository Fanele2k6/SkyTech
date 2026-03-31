using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace CM_Project.Models
{
    public class EmailServices
    {
        private readonly string fromEmail;
        private readonly string fromPassword;

        // IConfiguration replaces ConfigurationManager in Core
        // Register as scoped: builder.Services.AddScoped<Services>();
        public EmailServices(IConfiguration configuration)
        {
            fromEmail = configuration["EmailSettings:FromEmail"];
            fromPassword = configuration["EmailSettings:FromPassword"];
        }

        // ── Shared helper — avoids repeating the SmtpClient block ──
        private void Send(string toEmail, string subject, string body)
        {
            var fromAddress = new MailAddress(fromEmail, "Spaza Connect");
            var toAddress = new MailAddress(toEmail);

            using var smtp = new SmtpClient
            {
                Host = "smtp.office365.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };
            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };
            smtp.Send(message);
        }

        // ── Customer ─────────────────────────────────────────
        public void CustomerRegConfirmation(string toEmail, string firstName) =>
            Send(toEmail,
                "Welcome to Spaza Connect - Registration Successful",
                $"Dear {firstName},\n\n" +
                "Thank you for signing up with Spaza Connect. " +
                "Your account is now active and you can order your everyday essentials from the comfort of your home.\n" +
                "We're here to make shopping easier, faster and closer to your home.\n\n" +
                "Looking forward to serving you,\nSpaza Connect Team");

        // ── Driver ───────────────────────────────────────────
        public void DriverRegConfirmation(string toEmail, string firstName) =>
            Send(toEmail,
                "Welcome to Spaza Connect - Registration Successful",
                $"Dear {firstName},\n\n" +
                "Thank you for signing up with Spaza Connect to be part of our delivery driving community. " +
                "We will be reviewing your application and you will get feedback within 7 days.\n" +
                "Warm Regards,\nSpaza Connect Team");

        public void DriveAppliApproved(string toEmail, string firstName) =>
            Send(toEmail,
                "Welcome to Spaza Connect - Application Successful",
                $"Dear {firstName},\n\n" +
                "Thank you for applying to Spaza Connect. " +
                "You have been accepted as one of our delivery drivers.\n" +
                "Please log in to our system for more info on your delivery duties.\n" +
                "Warm Regards,\nSpaza Connect Team");

        public void DriveAppliReject(string toEmail, string firstName) =>
            Send(toEmail,
                "Spaza Connect - Application Outcome",
                $"Dear {firstName},\n\n" +
                "Thank you for applying to Spaza Connect. " +
                "Unfortunately your application was not successful.\n" +
                "Kind Regards,\nSpaza Connect Team");

        // ── Staff ────────────────────────────────────────────
        public void StaffRegConfirmation(string toEmail, string firstName) =>
            Send(toEmail,
                "Registration Succeeded",
                $"Dear {firstName},\n\n" +
                "You have been successfully registered as a staff member. " +
                "Welcome to the team!\n\nWarm Regards,\nSpaza Shop Manager");

        // ── OTP / Password reset ─────────────────────────────
        public void SendOtpEmail(string toEmail, string otp) =>
            Send(toEmail,
                "Your OTP Code",
                $"Your OTP code is: {otp}\nThis code will expire in 5 minutes.");

        // ── Shop Owner ───────────────────────────────────────
        public void SOwnerAppConfirmation(string toEmail, string firstName) =>
            Send(toEmail,
                "Welcome to Spaza Connect - Application Received",
                $"Dear {firstName},\n\n" +
                "Thank you for applying to Spaza Connect. " +
                "Your account is now active. Please check your application status in our application.\n" +
                "You will also receive an email on whether your application is approved or not.\n\n" +
                "Spaza Connect Team");

        public void SOwnerApplApproved(string toEmail, string firstName) =>
            Send(toEmail,
                "Spaza Connect - Application Approved",
                $"Dear {firstName},\n\n" +
                "Your application to sell on Spaza Connect has been approved.\n" +
                "Spaza Connect Team");

        public void SOwnerApplRejected(string toEmail, string firstName) =>
            Send(toEmail,
                "Spaza Connect - Application Outcome",
                $"Dear {firstName},\n\n" +
                "Thank you for applying to Spaza Connect. " +
                "Unfortunately your application has been rejected.\n" +
                "Please check your documents and ensure they are valid before trying again.\n\n" +
                "Spaza Connect Team");
    }
}
