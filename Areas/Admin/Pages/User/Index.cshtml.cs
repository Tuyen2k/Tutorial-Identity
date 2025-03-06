using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using System.Threading.Tasks;
using TutorialIdentity.Models;

namespace TutorialIdentity.Areas.Admin.Pages.User {
    [Authorize]
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<AppUser> _userManager;
        public IndexModel(UserManager<AppUser> userManager, ILogger<IndexModel> logger) {
            _userManager = userManager;
            _logger = logger;
        }

        public List<AppUser> Users { get; set; }

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
            if (!string.IsNullOrEmpty(search)) {

                Users = await _userManager.Users
                    .Where(u => u.UserName.Contains(search) || u.Email.Contains(search))
                    .Skip((p - 1) * ps)
                    .Take(ps)
                    .ToListAsync();

                total = await _userManager.Users
                    .Where(u => u.UserName.Contains(search) || u.Email.Contains(search))
                    .CountAsync();
            } else {
                Users = await _userManager.Users.Skip((p - 1) * ps).Take(ps).ToListAsync();
                total = await _userManager.Users.CountAsync();
            }

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
