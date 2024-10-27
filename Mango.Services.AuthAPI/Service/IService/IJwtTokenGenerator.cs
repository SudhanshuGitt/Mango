using Mango.Services.AuthAPI.Models;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IJwtTokenGenerator
    {
        // user can have multiple roles
        string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
    }
}
