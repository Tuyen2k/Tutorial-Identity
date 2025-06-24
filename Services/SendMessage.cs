

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Utils;

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

        // Gắn ảnh CID trước khi gọi ToMessageBody
        // Đường dẫn đến ảnh trong thư mục wwwroot/images
        var headerImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "hnc-email-header.png");
        var logoImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "hnc-logo.png");
        var footerImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "hnc-email-footer.png");

        // Thêm ảnh vào LinkedResources để sử dụng CID trong HTML
        var headerImage = builder.LinkedResources.Add(headerImagePath);
        var logoImage = builder.LinkedResources.Add(logoImagePath);
        var footerImage = builder.LinkedResources.Add(footerImagePath);

        // Gán CID cho các ảnh để sử dụng trong HTML
        headerImage.ContentId = "headerImage"; // Phải khớp với cid:headerLogo trong HTML
        logoImage.ContentId = "logoImage"; // Phải khớp với cid:headerLogo trong HTML
        footerImage.ContentId = "footerImage"; // Phải khớp với cid:headerLogo trong HTML

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
