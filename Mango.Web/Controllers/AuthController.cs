using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {

        private readonly IAuthService _authService;

        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDto loginRequestDto = new();
            return View(loginRequestDto);


        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            ResponseDto responseDto = await _authService.LoginAsync(loginRequestDto);
            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResonseDto loginResonseDto = JsonConvert.DeserializeObject<LoginResonseDto>(Convert.ToString(responseDto.Result));

                await SignInUser(loginResonseDto);
                _tokenProvider.SetToken(loginResonseDto.Token);

                return RedirectToAction("Index", "Home");

            }
            else
            {
                TempData["error"] = responseDto.Message;
                return View(loginRequestDto);
            }


        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.RoleAdmin, Value=SD.RoleAdmin},
                new SelectListItem{Text=SD.RoleCustomer,Value=SD.RoleCustomer}
            };

            ViewBag.RoleList = roleList;
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
        {
            ResponseDto result = await _authService.RegisterAsync(registrationRequestDto);
            ResponseDto assignRole;
            if (result != null && result.IsSuccess)
            {
                if (string.IsNullOrEmpty(registrationRequestDto.Role))
                {
                    registrationRequestDto.Role = SD.RoleCustomer;
                }
                assignRole = await _authService.AssignRoleAsync(registrationRequestDto);
                if (assignRole != null && assignRole.IsSuccess)
                {
                    TempData["success"] = "Registration Successful";
                    return RedirectToAction(nameof(Login));
                }

            }
            else
            {
                TempData["error"] = result.Message;
            }
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.RoleAdmin, Value=SD.RoleAdmin},
                new SelectListItem{Text=SD.RoleCustomer,Value=SD.RoleCustomer}
            };

            ViewBag.RoleList = roleList;

            return View(registrationRequestDto);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginResonseDto"></param>
        /// <returns></returns>
        private async Task SignInUser(LoginResonseDto loginResonseDto)
        {

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(loginResonseDto.Token);
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,
                jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value));

            identity.AddClaim(new Claim(ClaimTypes.Role,
                jwt.Claims.FirstOrDefault(x => x.Type == "role").Value));

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

    }
}
