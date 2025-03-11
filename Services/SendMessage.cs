

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

public class SendMessage : IEmailSender
{

    public readonly MailSetting _mailSetting;

    public SendMessage(MailSetting mailSetting)
    {
        _mailSetting = mailSetting;
    }
    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var email = new MimeMessage();
        email.Sender = new MailboxAddress(_mailSetting.DisplayName, _mailSetting.Mail);
        email.From.Add(new MailboxAddress(_mailSetting.DisplayName, _mailSetting.Mail));
        email.To.Add(new MailboxAddress(toEmail, toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder();
        builder.HtmlBody = htmlMessage;
        email.Body = builder.ToMessageBody();


        using var smtlClient = new SmtpClient();

        try
        {
            await smtlClient.ConnectAsync(_mailSetting.Host, _mailSetting.Port, SecureSocketOptions.StartTls);
            await smtlClient.AuthenticateAsync(_mailSetting.Mail, _mailSetting.Password);
            await smtlClient.SendAsync(email);
        }
        catch (Exception ex)
        {
            // Gửi mail thất bại, nội dung email sẽ lưu vào thư mục mailssave
            System.IO.Directory.CreateDirectory("mailssave");
            var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
            await email.WriteToAsync(emailsavefile);
            Console.WriteLine(ex.Message);
        }
        smtlClient.Disconnect(true);
    }
}
