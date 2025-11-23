using ExpenseVista.API.Configurations;
using ExpenseVista.API.DTOs.Auth;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

namespace ExpenseVista.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailService emailService;
        private readonly JwtService jwtService;
        private readonly ILookupNormalizer normalizer;
        private readonly ILogger<AuthService> logger;
        private readonly string frontendUrl;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            JwtService jwtService,
            ILookupNormalizer normalizer,
            IOptions<AppSettings> appOptions,
            ILogger<AuthService> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            this.jwtService = jwtService;
            this.normalizer = normalizer;
            this.logger = logger;
            frontendUrl = appOptions.Value.FrontendUrl;
        }

        public async Task<(bool Succeeded, List<string> Errors)> RegisterAsync(RegisterDTO registerDTO)
        {
            // Normalize the email using Identity logic
            var normalizedEmail = normalizer.NormalizeEmail(registerDTO.Email);

            // Check if user exists
            var existingUser = await userManager.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (existingUser != null)
                return (false, new List<string> { "Email is already in use." });

            // Create user
            var user = new ApplicationUser
            {
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName
            };

            var result = await userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return (false, errors);
            }

            await SendVerificationAsync(user);
        
            return (true, new List<string> {"Check your email for verification"});
        }

        public async Task SendVerificationAsync(ApplicationUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            logger.LogWarning($"THIS IS THE ENCODED TOKEN, COPY AND USE IN SWAGGER {encodedToken}");
            var verifyUrl = $"{frontendUrl}/verify-email?token={encodedToken}&email={user.Email}";
            await emailService.SendEmailAsync(user.Email!,  verifyUrl);
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            return result.Succeeded;
        }

        public async Task<(string, ApplicationUserDTO)> LoginAsync(LoginDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.EmailConfirmed)
            {

                throw new UnauthorizedAccessException("Invalid credentials or email not verified.");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {

                throw new UnauthorizedAccessException("Invalid credentials.");
            }
            // Success: Generate Token and DTO
            var token = jwtService.GenerateToken(user);

            var applicationUserDTO = new ApplicationUserDTO
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
            };

            return (token, applicationUserDTO);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            // Always return success (to avoid account enumeration)
            if (user == null)
                return;

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var resetUrl = $"{frontendUrl}/reset-password?token={encodedToken}&email={user.Email}";

            await emailService.SendPasswordResetEmailAsync(user.Email!, resetUrl);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null) return false;

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));

            var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

            if(result.Succeeded)
            {
                await emailService.SendPasswordChangedNotification(user.Email!);
                return true;
            }
            return false;
        }

    }
}
