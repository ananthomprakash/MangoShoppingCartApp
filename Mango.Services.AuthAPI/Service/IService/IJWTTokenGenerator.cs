using Mango.Services.AuthAPI.Models;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IJWTTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUser);
    }
}
