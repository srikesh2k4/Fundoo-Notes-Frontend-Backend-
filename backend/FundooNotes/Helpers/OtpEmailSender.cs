// ========================================
// FILE: FundooNotes/Helpers/OtpEmailSender.cs
// ========================================
using Microsoft.Extensions.Options;
using ModelLayer.Configuration;
using System.Net;
using System.Net.Mail;

namespace FundooNotes.Helpers
{
    public class OtpEmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<OtpEmailSender> _logger;

        public OtpEmailSender(IOptions<SmtpSettings> smtpSettings, ILogger<OtpEmailSender> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string userName, string otp)
        {
            try
            {
                using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                    Subject = "Verify Your Email - Fundoo Notes",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #1976D2;'>Hello {userName},</h2>
                                <p>Thank you for registering with Fundoo Notes!</p>
                                <p>Your OTP for email verification is:</p>
                                <div style='background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                                    {otp}
                                </div>
                                <p style='color: #666;'>This OTP will expire in 10 minutes.</p>
                                <p style='color: #666;'>If you didn't request this, please ignore this email.</p>
                                <br/>
                                <p>Best regards,<br/><strong>Fundoo Notes Team</strong></p>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"OTP email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {toEmail}");
                throw new Exception("Failed to send verification email", ex);
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            try
            {
                using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                    Subject = "Password Reset - Fundoo Notes",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #1976D2;'>Password Reset Request</h2>
                                <p>You requested to reset your password.</p>
                                <p>Your reset token is:</p>
                                <div style='background-color: #f5f5f5; padding: 15px; word-break: break-all; font-family: monospace; margin: 20px 0;'>
                                    {resetToken}
                                </div>
                                <p style='color: #666;'>This token will expire in 15 minutes.</p>
                                <p style='color: #666;'>If you didn't request this, please ignore this email.</p>
                                <br/>
                                <p>Best regards,<br/><strong>Fundoo Notes Team</strong></p>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Password reset email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {toEmail}");
                throw new Exception("Failed to send password reset email", ex);
            }
        }
    }
}