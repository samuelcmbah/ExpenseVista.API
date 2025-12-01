using ExpenseVista.API.Common.Exceptions;
using ExpenseVista.API.DTOs.Auth;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExpenseVista.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IAuthService authService;
        private readonly ILogger<AuthController> logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuthService authService,
            ILogger<AuthController> logger
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authService = authService;
            this.logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var (succeeded, errors) = await authService.RegisterAsync(registerDTO);

            if (!succeeded)
            {
                // Keep the same format your frontend expects
                throw new BadRequestException(string.Join(", ", errors));
            }

            return Ok(new { message = "User registered successfully" });
        }


        [HttpPost("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
        {

            var succeeded = await authService.ConfirmEmailAsync(verifyEmailDTO.Email, verifyEmailDTO.Token);
            if (!succeeded)
            {
                throw new BadRequestException("Verification failed.");
            }

            return Ok(new { message = "Email has been verified." });

        }

        [HttpPost("resend-verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerification([FromBody] EmailRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new BadRequestException("User not found.");

            if (user.EmailConfirmed)
                throw new BadRequestException("Email already verified.");

            await authService.SendVerificationAsync(user);

            return Ok(new { message = "Verification email resent." });
        }



        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
           
                var (token, applicationUser) = await authService.LoginAsync(loginDTO);

                // Success: Return 200 OK with the token and user data
                logger.LogInformation("login succeeded");
                return Ok(new { token, applicationUser });
           
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            await authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "If an account exists, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var success = await authService.ResetPasswordAsync(dto);

            if (!success)
                throw new BadRequestException("Invalid or expired token.");

            return Ok(new { message = "Password reset successful" });
        }
    }

    
}
