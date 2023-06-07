using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        //here we need two accounts 1. register an account 2. login

        private readonly IAuthService _authService;
        protected ResponseDto _response;

        public AuthAPIController(IAuthService authService)
        {
                _authService = authService;
            _response= new ResponseDto();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto registrationRequestDto) 
        {
            var errorMessage = await _authService.Register(registrationRequestDto);
            if(!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }
            
            
            return Ok(_response);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            return Ok();
        }


    }
}
