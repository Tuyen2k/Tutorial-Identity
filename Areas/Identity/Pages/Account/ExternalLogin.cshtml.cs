// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;

namespace TutorialIdentity.Areas.Identity.Pages.Account {
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender) {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null) {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null) {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null) {
                ErrorMessage = $"Lỗi từ nhà cung cấp bên ngoài: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                ErrorMessage = "Có lỗi khi tải thông tin đăng nhập bên ngoài.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            // Login thành công: redirect về url
            // TK đang bị khóa: redirect về trang khóa tk
            // Lấy thông tin trong info từ provider và gán vào form

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded) {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut) {
                return RedirectToPage("./Lockout");
            } else {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email)) {
                    Input = new InputModel {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null) {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                ErrorMessage = "Có lỗi khi tải thông tin đăng nhập bên ngoài trong quá trình xác nhận.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid) {
                // info: 
                // kiểm tra tk với email từ provider:
                // 1. Chưa có tk : 
                // nếu Input.Email == emalExternal --> tạo mới tài khoản
                // nếu Input.Email != emailExternal --> return lỗi 
                // 2. Đã có tk --> check: Id của tk Input.Email nếu có == Id của tk email từ provider
                // nếu == thì tạo liên kết với external
                // nếu != return lỗi

                var emailExternal = info.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var userExternal = await _userManager.FindByEmailAsync(emailExternal);
                var userInput = await _userManager.FindByEmailAsync(Input.Email);


                if (userExternal == null) {
                    if (emailExternal == Input.Email) {
                        var userCreate = CreateUser();
                        await _userStore.SetUserNameAsync(userCreate, Input.Email, CancellationToken.None);
                        await _emailStore.SetEmailAsync(userCreate, Input.Email, CancellationToken.None);

                        // tạo user
                        var result = await _userManager.CreateAsync(userCreate);
                        if (result.Succeeded) {
                           return await HandleAddLoginUserAsync(userCreate, info, returnUrl);
                        }
                    } else {
                        ErrorMessage = "Có lỗi khi tải thông tin đăng nhập bên ngoài trong quá trình xác nhận.";
                        return Page();
                    }
                } else if (userInput != null & userExternal.Id == userInput.Id) {

                    return await HandleAddLoginUserAsync(userExternal, info, returnUrl);

                } else {
                    ErrorMessage = "Có lỗi khi tải thông tin đăng nhập bên ngoài trong quá trình xác nhận.";
                    return Page();
                }
            }

            return Content("Stop here");
        }

        private async Task<IActionResult> HandleAddLoginUserAsync(AppUser user, ExternalLoginInfo info, string returnUrl = "/") {
            // thêm login external cho user
            var result = await _userManager.AddLoginAsync(user, info);

            if (result.Succeeded) {
                // thêm logic result.Succeeded
                if (!user.EmailConfirmed) {
                    // Lấy thông tin user và gửi email yêu cầu xác thực email
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                }

                // nếu yêu cầu xác thực email để đăng nhập
                if (_userManager.Options.SignIn.RequireConfirmedAccount) {
                    return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                }
                // không yc xác thực email thì đăng nhập luôn và redirect về url
                await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            ErrorMessage = "Có lỗi khi tải thông tin đăng nhập bên ngoài trong quá trình xác nhận.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });

        }

        private AppUser CreateUser() {
            try {
                return Activator.CreateInstance<AppUser>();
            } catch {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore() {
            if (!_userManager.SupportsUserEmail) {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
