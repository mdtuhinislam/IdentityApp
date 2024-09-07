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
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;

namespace IdentityApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AccountController(JWTService jwtService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration config,
            EmailService emailService)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _emailService = emailService;
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
                FirstName = registerDto.FirstName.ToLower(),
                LastName = registerDto.LastName.ToLower(),
                Email = registerDto.Email.ToLower(),
                UserName = registerDto.Email.ToLower(),
                //EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(userToAdd, registerDto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            try
            {
                if (await SendConfirmationEmailAsync(userToAdd))
                {
                    return Ok(new JsonResult(new
                    {
                        title = "Account Created",
                        message = "Your account has been created, please confirm your email"
                    }));
                }
                return BadRequest("Email send failed!");
            }
            catch (Exception ex)
            {
                return BadRequest("Email send failed!" + ex.Message.ToString());
            }
            //return Ok("Your account has created successfully!, you can login");
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

        private async Task<bool> SendConfirmationEmailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = $"{_config["JWT:ClientUrl"]}{_config["Email:ConfirmEmailPath"]}?toke={token}&email={user.Email}";

            var body = $"<p>Hello {user.FirstName} {user.LastName}</p>" +
                "<p>Please confirm your email by clicking on the following link</p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                "<p>Thank you</p>" +
                $"<br>{_config["Email:ApplicationName"]}";
            var emailSend = new EmailSendDto(user.Email, "Confirm your email.", body);
            return await _emailService.SendEmailAsync(emailSend);
        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync(EmailConfirmDto emailConfirm)
        {
            var user = await _userManager.FindByEmailAsync(emailConfirm.Email);

            if (user == null)
                return Unauthorized("This email has not been registered.");
            if (user.EmailConfirmed == true)
                return BadRequest("Your email has confirmed before, please login.");
            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(emailConfirm.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if(result != null)
                {
                    if (result.Succeeded)
                        return Ok(new JsonResult(new
                        {
                            title = "Your email confirmed!",
                            message = "Email has confirmed you can login now."
                        }));
                }
                return BadRequest("Invalid token, please try again later.");
            }
            catch(Exception)
            {
                return BadRequest("Invalid token, please try again later.");
            }

        }
        [HttpPost("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResentEmailConfirmationLink(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Invalid email addess.");
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized("You are not a registered user, please get registered firs.");
            if (user.EmailConfirmed == true)
                return BadRequest("Your email has been validated once, you can login.");
            try
            {
                if (await SendConfirmationEmailAsync(user))
                    return Ok(new JsonResult(new
                    {
                        title = "Confirmation link sent.",
                        message= "Please confirm your email address."
                    }));
                return BadRequest("Failed to send email, please try again!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send email, please try again!, exception occured with: {ex.Message.ToString()}");
            }
        }

        [HttpPost("forgot-user-or-password/{email}")]
        public async Task<IActionResult> ForgotUserNameOrPassword(string email)
        {
            
            try
            {
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Invalid email addess.");
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return Unauthorized("You are not a registered user, please get registered firs.");
                if (user.EmailConfirmed == false)
                    return BadRequest("This email is not registered!. Please get registered first.");
                if (await SendForgotUserNameOrPasswordEmail(user))
                    return Ok(new JsonResult(new { title = "Forgot user or password email sent.", message = "Please check your email." }));
                return BadRequest("Faild to send email, contact admin.");
            }
            catch (Exception)
            {
                return BadRequest("Faild to send email, contact admin.");
            }

        }



        private async Task<bool> SendForgotUserNameOrPasswordEmail(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = $"{_config["JWT:ClientUrl"]}{_config["Email:ConfirmEmailPath"]}?toke={token}&email={user.Email}";

            var body = $"<p>Hello {user.FirstName} {user.LastName}</p>" +
                $"<p>User Name : {user.UserName}</p>" +
                $"<p>Please click following link to reset password</p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                "<p>Thank you</p>" +
                $"<br>{_config["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email, "Confirm your email.", body);
            return await _emailService.SendEmailAsync(emailSend);
        }






        #endregion


    }
}
