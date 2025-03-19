using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TutorialIdentity.Models;

namespace TutorialIdentity.Areas.Admin.Pages.Role {
    [Authorize(Roles ="Admin")]
    public class DetailModel : PageModel {
        private readonly RoleManager<AppRole> _roleManager;
        public DetailModel(RoleManager<AppRole> roleManager) {
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; } = "";

        [BindProperty]
        public InputModel Input { get; set; }

        public bool IsDetail { get; set; } = false;
        public bool IsCreate { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(string? roleId, bool isDetail) {
            if (string.IsNullOrEmpty(roleId)) {
                IsCreate = true;
                ViewData["Title"] = "Thêm mới vai trò người dùng";
                return Page();
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) {
                StatusMessage = $"Không tìm thấy vai trò người dùng với ID: {roleId}";
                return Page();
            }

            Input = new InputModel {
                Name = role.Name,
            };

            Input.Id = roleId;

            if (isDetail) {
                IsDetail = true;
                ViewData["Title"] = "Chi tiết vai trò người dùng";
            } else {
                ViewData["Title"] = "Chỉnh sửa vai trò người dùng";
            }


            return Page();
        }

        public async Task<IActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                return Page();
            }

            var id = Input.Id;

            AppRole role;
            if (string.IsNullOrEmpty(id)) {

                if (await _roleManager.FindByNameAsync(Input.Name) != null) {
                    StatusMessage = $"Vai trò {Input.Name} đã tồn tại";
                    return Page();
                }

                role = new AppRole {
                    Name = Input.Name,
                    IsDeleted = false,
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded) {
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            } else {

                role = await _roleManager.FindByIdAsync(id);
                if (role == null) {
                    StatusMessage = $"Không tìm thấy vai trò người dùng với ID: {id}";
                    return Page();
                }

                role.Name = Input.Name;
                
                var result = await _roleManager.UpdateAsync(role);
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
        [Display(Name = "Tên vai trò")]
        public string Name { get; set; }
        public string? Id { get; set; }
    }

}
