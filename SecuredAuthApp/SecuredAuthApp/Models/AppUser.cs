using Microsoft.AspNetCore.Identity;

namespace SecuredAuthApp.Models
{
    public class AppUser:IdentityUser
    {
        public string? FullName { get; set; }
    }
}
