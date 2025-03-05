using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="p">trang hiện tại</param>
        /// <param name="ps">số lượng bản ghi mỗi trang</param>
        /// <returns></returns>
        public async Task OnGet(int p = 0, int ps = 10) {
            Users = await _userManager.Users.Skip(p * ps).Take(ps).ToListAsync();
            CurrentCount = (p * ps) + 1;

            var total = await _userManager.Users.CountAsync();
            var totalPage = (int) Math.Ceiling((double)total / ps);

            Pagination = new Pagination {
                CurrentPage = p,
                TotalPages = total,
                GetPageUrl = p => Url.Page("", new { p })
            };
        }
    }
}
