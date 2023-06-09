namespace Mango.Services.AuthAPI.Models.Dto
{
    public class LoginResonseDto
    {
        public UserDto User { get; set; }

        public string Token { get; set; }
    }
}
