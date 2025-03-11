using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using System.ComponentModel.DataAnnotations;
using TutorialIdentity.Models;

namespace TutorialIdentity.Areas.Admin.Pages.User {
    [Authorize(Roles = "Admin,Editor")]
    public class DetailModel : PageModel {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        public DetailModel(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; } = "";

        [BindProperty]
        public InputModel Input { get; set; }

        public bool IsDetail { get; set; } = false;
        public bool IsCreate { get; set; } = false;
        public List<string> Roles { get; set; }
        public SelectList SelectListRoles { get; set; }


        public async Task<IActionResult> OnGetAsync(string? userId, bool isDetail) {

            Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            SelectListRoles = new SelectList(Roles);

            if (string.IsNullOrEmpty(userId)) {
                Input = new InputModel();
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
                Password = "q123456",
            };

            

            Input.Id = userId;

            if (isDetail) {
                IsDetail = true;
                ViewData["Title"] = "Chi tiết người dùng";
            } else {
                ViewData["Title"] = "Chỉnh sửa người dùng";
            }

            
            Input.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            
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
                var result = await _userManager.CreateAsync(user, string.IsNullOrEmpty(Input.Password) ? "q123456" : Input.Password );
                if (!result.Succeeded) {
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
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

                if (!Input.Password.Equals("q123456") && !string.IsNullOrEmpty(Input.Password)) {
                    var removePassword = await _userManager.RemovePasswordAsync(user);
                    var updatePassword = await _userManager.AddPasswordAsync(user, Input.Password);
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) {
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    return Page();
                }
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = Input.Roles.Except(userRoles).ToList();
            var rolesToRemove = userRoles.Except(Input.Roles).ToList();

            if (rolesToAdd.Any()) {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            if (rolesToRemove.Any()) {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
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


        [StringLength(100, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Vai trò")]
        public List<string> Roles { get; set; } = [];
    }

}
