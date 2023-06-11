using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {

        private readonly AppDbContext _db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IJWTTokenGenerator _jwtTokenGenerator;

        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,IJWTTokenGenerator jWTTokenGenerator)
        {
                _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jWTTokenGenerator;
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user= _db.ApplicationUsers.FirstOrDefault(u=>u.UserName.ToLower() == email.ToLower());
            if(user != null)
            {
                if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                {
                    //create role if it does not exist
                    _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;
        }


        public async Task<LoginResonseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => 
            u.UserName.ToLower() ==
            loginRequestDto.UserName.ToLower());
            bool isValid=await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if(user ==null || isValid == false)
            {
                return new LoginResonseDto { User = null, Token = "" };
            }
            //if the user is found we need to generate the token
            //we want to generate the token for the logged in user
           var token= _jwtTokenGenerator.GenerateToken(user);

            UserDto userDto = new()
            {
                Email = user.Email,
                ID = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            LoginResonseDto loginResonseDto = new LoginResonseDto()
            {
                User = userDto,
                Token = token
            };

            return loginResonseDto;

        }

        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new ApplicationUser()
            {
                Name=registrationRequestDto.Name,
                Email=registrationRequestDto.Email,
                NormalizedEmail=registrationRequestDto.Email.ToUpper(),
                UserName=registrationRequestDto.Email,
                PhoneNumber=registrationRequestDto.PhoneNumber
            };

            try
            {
                var result = await  _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);

                    UserDto userDto = new UserDto()
                    {
                        Email=userToReturn.Email,
                        ID=userToReturn.Id,
                        Name=userToReturn.Name,
                        PhoneNumber =userToReturn.PhoneNumber

                    };

                    return "";

                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }
            }
            catch (Exception ex)
            {

            }
            return "Error Encountered";

        }
    }
}
