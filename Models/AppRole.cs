using Microsoft.AspNetCore.Identity;

namespace TutorialIdentity.Models
{
    public class AppRole : IdentityRole
    {
        public bool IsDeleted { get; set; }
    }
}