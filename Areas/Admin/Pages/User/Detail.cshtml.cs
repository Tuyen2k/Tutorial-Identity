using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using System.ComponentModel.DataAnnotations;

namespace TutorialIdentity.Areas.Admin.Pages.User {
    [Authorize]
    public class DetailModel : PageModel {
        private readonly UserManager<AppUser> _userManager;
        public DetailModel(UserManager<AppUser> userManager) {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; } = "";

        [BindProperty]
        public InputModel Input { get; set; }

        public bool IsDetail { get; set; } = false;
        public bool IsCreate { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(string? userId, bool isDetail) {
            if (string.IsNullOrEmpty(userId)) {
                IsCreate = true;
                ViewData["Title"] = "Thêm mới người dùng";
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) {
                StatusMessage = $"Không tìm thấy người dùng với ID: {userId}";
                return Page();
            }

            Input = new InputModel {
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
            };

            Input.Id = userId;

            if (isDetail) {
                IsDetail = true;
                ViewData["Title"] = "Chi tiết người dùng";
            } else {
                ViewData["Title"] = "Chỉnh sửa người dùng";
            }


            return Page();
        }

        public async Task<IActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                return Page();
            }

            var id = Input.Id;

            AppUser user;
            if (string.IsNullOrEmpty(id)) {

                if (await _userManager.FindByEmailAsync(Input.Email) != null) {
                    StatusMessage = $"Email {Input.Email} đã tồn tại";
                    return Page();
                }

                user = new AppUser {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    FullName = Input.FullName,
                    Address = Input.Address,
                    Avatar = "",
                    IsDeleted = false,
                    EmailConfirmed = true,
                    Id = Guid.NewGuid().ToString(),
                    PhoneNumber = Input.PhoneNumber,
                    LockoutEnabled = true,
                };
                var result = await _userManager.CreateAsync(user, "q123456");
                if (!result.Succeeded) {
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            } else {

                user = await _userManager.FindByIdAsync(id);
                if (user == null) {
                    StatusMessage = $"Không tìm thấy người dùng với ID: {id}";
                    return Page();
                }

                user.UserName = Input.UserName;
                user.Email = Input.Email;
                user.FullName = Input.FullName;
                user.Address = Input.Address;
                user.PhoneNumber = Input.PhoneNumber;


                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) {
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }
    }

    public class InputModel {
        [Required(ErrorMessage = "{0} là bắt buộc")]
        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; }
        public string? Id { get; set; }

        [Required(ErrorMessage = "{0} là bắt buộc")]
        [EmailAddress(ErrorMessage = "{0} không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} là bắt buộc")]
        [Display(Name = "Tên đầy đủ")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "{0} là bắt buộc")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Phone]
        [RegularExpression(@"^(?:\+84|0)(3[2-9]|5[2689]|7[0-9]|8[1-9]|9[0-9])\d{7}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        //[Required(ErrorMessage = "{0} không được để trống")]
        //[StringLength(100, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Mật khẩu")]
        //public string Password { get; set; }
    }

}
