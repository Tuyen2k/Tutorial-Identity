using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using System.Threading.Tasks;
using TutorialIdentity.Models;

namespace TutorialIdentity.Areas.Admin.Pages.User {
    [Authorize(Roles ="Admin")]
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<AppUser> _userManager;
        public IndexModel(UserManager<AppUser> userManager, ILogger<IndexModel> logger) {
            _userManager = userManager;
            _logger = logger;
        }

        public class UserAndRole : AppUser {
            public string? Roles { get; set; }
        }

        public List<UserAndRole> Users { get; set; }

        public int CurrentCount { get; set; }
        public Pagination Pagination { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="p">trang hiện tại</param>
        /// <param name="ps">số lượng bản ghi mỗi trang</param>
        /// <returns></returns>
        public async Task OnGet(string? search, int p = 1, int ps = 10) {
            if (p < 1) p = 1;
            if (ps < 1) ps = 10;

            int total;
            List<AppUser> users;
            if (!string.IsNullOrEmpty(search)) {

                users = await _userManager.Users
                    .Where(u => u.UserName.Contains(search) || u.Email.Contains(search))
                    .OrderBy(u => u.UserName)
                    .Skip((p - 1) * ps)
                    .Take(ps)
                    .ToListAsync();
                    

                total = await _userManager.Users
                    .Where(u => u.UserName.Contains(search) || u.Email.Contains(search))
                    .CountAsync();
            } else {
                users = await _userManager.Users.OrderBy(u => u.UserName).Skip((p - 1) * ps).Take(ps).ToListAsync();
                total = await _userManager.Users.CountAsync();
            }

            Users = users.Select(u => new UserAndRole {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        FullName = u.FullName,
                        Address = u.Address,
                        PhoneNumber = u.PhoneNumber,
                        AccessFailedCount = u.AccessFailedCount,
                        Avatar = u.Avatar,
                        IsDeleted = u.IsDeleted,
                        ConcurrencyStamp = u.ConcurrencyStamp,
                        LockoutEnabled = u.LockoutEnabled,
                        EmailConfirmed = u.EmailConfirmed,
                        PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                        TwoFactorEnabled = u.TwoFactorEnabled,
                        LockoutEnd = u.LockoutEnd,
                        NormalizedEmail = u.NormalizedEmail,
                        NormalizedUserName = u.NormalizedUserName,
                        PasswordHash = u.PasswordHash,
                        SecurityStamp = u.SecurityStamp
                    }).ToList();

            Users.ForEach(u => {
                u.Roles = string.Join(", ", _userManager.GetRolesAsync(u).Result);
            });

            CurrentCount = ((p - 1) * ps) + 1;
            var totalPage = (int)Math.Ceiling((double)total / ps);

            Pagination = new Pagination {
                CurrentPage = p,
                TotalPages = totalPage,
                PageSize = ps,
                GetPageUrl = (p, ps) => Url.Page("", new { p, ps })
            };
        }

        public async Task<IActionResult> OnPostDeleteAsync(string userId) {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) {
                return NotFound();
            }

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) {
                ModelState.AddModelError(string.Empty, "Xóa người dùng thất bại.");
                return Page();
            }

            StatusMessage = "Xóa người dùng thành công.";
            return RedirectToPage();
        }

    }
}
