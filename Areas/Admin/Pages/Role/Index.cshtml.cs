

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TutorialIdentity.Models;

namespace TutorialIdentity.Areas.Admin.Pages.Role {
    [Authorize(Roles ="Admin")]
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;
        private readonly RoleManager<AppRole> _roleManager;
        public IndexModel(RoleManager<AppRole>  roleManager, ILogger<IndexModel> logger) {
            _roleManager = roleManager;
            _logger = logger;
        }

        public List<AppRole> Roles { get; set; }

        public int CurrentCount { get; set; }
        public Pagination Pagination { get; set; }
        public string Search { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Get all Roles
        /// </summary>
        /// <param name="p">trang hiện tại</param>
        /// <param name="ps">số lượng bản ghi mỗi trang</param>
        /// <returns></returns>
        public async Task OnGetAsync(string? search, int p = 1, int ps = 10) {
            if (p < 1) p = 1;
            if (ps < 1) ps = 10;

            int total;
            if (!string.IsNullOrEmpty(search)) {
                Search = search;
                Roles = await _roleManager.Roles
                    .Where(u => u.Name.Contains(search))
                    .Skip((p - 1) * ps)
                    .Take(ps)
                    .ToListAsync();

                total = await _roleManager.Roles
                    .Where(u => u.Name.Contains(search))
                    .CountAsync();
            } else {
                Roles = await _roleManager.Roles.Skip((p - 1) * ps).Take(ps).ToListAsync();
                total = await _roleManager.Roles.CountAsync();
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
            var user = await _roleManager.FindByIdAsync(userId);
            if (user == null) {
                return NotFound();
            }

            user.IsDeleted = true;
            var result = await _roleManager.UpdateAsync(user);
            if (!result.Succeeded) {
                ModelState.AddModelError(string.Empty, "Xóa người dùng thất bại.");
                return Page();
            }

            StatusMessage = "Xóa người dùng thành công.";
            return RedirectToPage();
        }

    }
}
