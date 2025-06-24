using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TutorialIdentity.Pages;

public class SendEmailModel : PageModel
{
    private readonly ILogger<SendEmailModel> _logger;
    private readonly IEmailSender _emailSender;

    public SendEmailModel(ILogger<SendEmailModel> logger, IEmailSender emailSender)
    {
        _logger = logger;
        _emailSender = emailSender;
    }

    public void OnGet()
    {
        ViewData["Title"] = "Send Email OnGet";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var htmlBody = GenerateThanksEmailBody("Software Engineer", "Nguyen Van A");
        await _emailSender.SendEmailAsync("kaiba132k@gmail.com", "Test HNC email", htmlBody);

        // Có thể redirect hoặc hiển thị thông báo
        TempData["Message"] = "Email đã được gửi thành công!";
        return Page();

    }

    private string GenerateThanksEmailBody(string jobTitle, string applicantName)
    {
        var htmlBody = @"
            <div style=""max-width: 800px; margin: auto; background-color: #fff; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);"">
            <div class=""container-email"">
                <div class=""hnc-email-header"">
                    <img src=""cid:headerImage"" alt=""hnc-logo"" style=""width: 100%; height: auto;"">
                </div>
                <div class=""hnc-email-body"" style=""padding: 30px 60px;"">
                    <div class=""title"" style=""font-size: x-large; font-weight: bold; color: #333; margin-bottom: 20px;"">
                        THƯ CẢM ƠN VÀ PHẢN HỒI HỒ SƠ ỨNG TUYỂN TẠI HNC
                    </div>

                    <div class=""body"" style=""line-height: 2.5;"">
                        <p>Chào [Tên ứng viên],<br />
                            Trước tiên, HNC xin gửi lời cảm ơn chân thành đến bạn vì đã tin tưởng và dành thời gian ứng tuyển vào vị trí [Tên vị trí].<br />
                            Sau quá trình đánh giá kỹ lưỡng, HNC rất tiếc khi phải thông báo rằng hiện tại, chúng tôi chưa thể chọn bạn đồng hành cùng đội ngũ. Quyết định này hoàn toàn không phản ánh năng lực hay tiềm năng của bạn, mà chỉ đơn giản là dựa trên sự phù hợp với nhu cầu tuyển dụng tại thời điểm này.<br />
                            Chúng tôi rất trân trọng những chia sẻ và hành trình sự nghiệp của bạn. Mong rằng trong tương lai, chúng ta sẽ có cơ hội gặp lại nhau ở những thời điểm phù hợp hơn.<br />
                            Chúc bạn luôn vững bước và đạt được những điều tuyệt vời nhất trên hành trình của mình!
                        </p>
                        <p>Thân mến,</p>
                        <img src=""cid:logoImage"" alt=""hnc-logo"" style=""width: 100px; height: auto;"">
                        <img src=""cid:footerImage"" alt=""hnc-footer"" style=""width: 100%; height: auto;"">
                    </div>
                </div>
            </div>
        </div>";

        htmlBody = htmlBody.Replace("[Tên ứng viên]", applicantName)
                            .Replace("[Tên vị trí]", jobTitle);

        return htmlBody;
    }
}

