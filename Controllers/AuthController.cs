using ExpenseVista.API.Common.Exceptions;
using ExpenseVista.API.DTOs.Auth;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
    public class AuthController : BaseController
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

        private void SetRefreshTokenCookie(TokenResponseDTO tokenResponse)
        {
            // CRITICAL: If SameSite is None, Secure MUST be true.
            // Even on localhost, if you are doing Cross-Origin (Port 5000 to 7000), use this.
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = tokenResponse.RefreshTokenExpiresAt,
            };
            Response.Cookies.Append("refreshToken", tokenResponse.RefreshToken, cookieOptions);
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

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {

            var result = await authService.LoginAsync(loginDTO);

            // Success: Return 200 OK with the token and user data
            logger.LogInformation("login succeeded");

            SetRefreshTokenCookie(result.tokenResponse);
            //return only access token and user info to frontend
            return Ok(new 
            { 
                token = new { accessToken = result.tokenResponse.AccessToken},
                user = result.applicationUserDTO
            });

        }

        [HttpPost("google-login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            var result = await authService.GoogleLoginAsync(request);

            // This logic is identical to your email login, we can centralize it
            SetRefreshTokenCookie(result.tokenResponse);

            return Ok(new
            {
                token = new { accessToken = result.tokenResponse.AccessToken },
                user = result.applicationUserDTO
            });
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh()
        {
            // 1. Read from Cookie
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("No refresh token provided");
            }
            // 2. Call the updated service (passing only the string)
            var tokenResponse = await authService.RefreshTokenAsync(refreshToken);

            // 3. Set the NEW cookie (Rotation)
            SetRefreshTokenCookie(tokenResponse);
            // 4. Return the new Access Token
            return Ok(new { accessToken = tokenResponse.AccessToken });
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // If user is authenticated, revoke tokens for current user
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await authService.LogoutAsync(userId);
            }

            // Overwrite the cookie with an expired one
            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            });
            return Ok(new { message = "Logged out" });
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
