using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Models
{
    // adding custom properties to user table
    // as default user is Identity user application user will extend Indentiy user
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
