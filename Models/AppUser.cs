using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Avatar { get; set; } = "";
    public bool IsDeleted { get; set; } = false;
}
