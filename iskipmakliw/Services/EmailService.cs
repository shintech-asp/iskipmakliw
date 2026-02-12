using System.Net;
using System.Net.Mail;

namespace iskipmakliw.Services
{
    public class EmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _fromEmail = "yhujiinn@gmail.com";
        private readonly string _fromPassword = "vgxx fish qxgq liut";
        private readonly string _fromName = "OakMart";
        public bool SendVerificationCode(string toEmail, string code)
        {
            try
            {
                string subject = "Your Verification Code";
                string body = GetEmailBody(code);

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail, _fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        private string GetEmailBody(string code)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Email Verification</h2>
                    <p>Thank you for registering! Please use the following code to verify your email address:</p>
                    
                    <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0;'>
                        <h1 style='color: #667eea; font-size: 48px; margin: 0; letter-spacing: 10px;'>{code}</h1>
                    </div>
                    
                    <p><strong>This code will expire in 10 minutes.</strong></p>
                    <p>If you didn't request this code, please ignore this email.</p>
                    
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                    <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </div>
            </body>
            </html>
        ";
        }
    }
}
