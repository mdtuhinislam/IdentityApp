using IdentityApp.Services;
using IdentityApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentityApp.DTOs.Account;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IdentityApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(JWTService jwtService,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return Unauthorized("Invalid username or password");
            if (user.EmailConfirmed == false)
                return Unauthorized("Please confirm your email");
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid username or password");
            return CreateApplicationUserDto(user);
        }
        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterDto registerDto)
        {
            if (await CheckEmailExsitsAsync(registerDto.Email))
                return BadRequest($"This {registerDto.Email} is used by another user, please enter an email except this one!");
            var userToAdd = new User
            {
                FirstName = registerDto.FistName.ToLower(),
                LastName = registerDto.LastName.ToLower(),
                Email = registerDto.Email.ToLower(),
                UserName = registerDto.Email.ToLower(),
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(userToAdd, registerDto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok("Your account has created successfully!, you can login");
        }
        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return CreateApplicationUserDto(user);
        }


        #region Private Helper Method
        private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = _jwtService.CreateJWTToken(user)
            };
        }

        private async Task<bool> CheckEmailExsitsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }
        #endregion


    }
}
