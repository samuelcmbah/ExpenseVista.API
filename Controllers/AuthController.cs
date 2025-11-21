using ExpenseVista.API.DTOs.Auth;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services;
using ExpenseVista.API.Services.IServices;
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
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (registerDTO.Password != registerDTO.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            var (succeeded, errors) = await authService.RegisterAsync(registerDTO);

            if (!succeeded)
            {
                // Keep the same format your frontend expects
                return BadRequest(new { message = "Registration failed.", errors });
            }

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
        {
            try
            {
                var succeeded = await authService.ConfirmEmailAsync(verifyEmailDTO.Email, verifyEmailDTO.Token);
                return Ok(new { mesage = "Email has been verified" });
            }
            catch (Exception ex) { 
                logger.LogError(ex.Message);
                return BadRequest(new { message = "verification failed.", ex.Message });
            }
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendEmail([FromQuery] string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
                return BadRequest(new { message = "User not found." });

            if (user.EmailConfirmed)
                return BadRequest(new { message = "Email already verified." });

            await authService.SendVerificationAsync(user);

            return Ok(new { message = "Verification email resent." });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                var (token, applicationUser) = await authService.LoginAsync(loginDTO);

                // Success: Return 200 OK with the token and user data
                return Ok(new { token, applicationUser });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Failure: Return 401 Unauthorized with the message from the service
                // The service throws this exception for both bad email/password and unverified email.
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle unexpected server errors
                logger.LogError($"Unexpected login error {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred during login." });
            }
        }
    }
}
