using ExpenseVista.API.Common.Exceptions;
using ExpenseVista.API.Configurations;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Auth;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
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
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext dbContext;
        private readonly string frontendUrl;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            JwtService jwtService,
            ILookupNormalizer normalizer,
            IOptions<AppSettings> appOptions,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            this.jwtService = jwtService;
            this.normalizer = normalizer;
            this.logger = logger;
            this.configuration = configuration;
            this.dbContext = dbContext;
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
                throw new BadRequestException("Email is already in use.");

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

        public async Task<(TokenResponseDTO tokenResponse, ApplicationUserDTO applicationUserDTO)> LoginAsync(LoginDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (!user.EmailConfirmed)
            {
                throw new UnauthorizedAccessException("EMAIL_NOT_CONFIRMED");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }
            // Success: Generate Token 
            var accessToken = jwtService.GenerateAccessToken(user, out DateTime accessExpiresAt);

            // Generate refresh token string (raw) and hash for DB
            var refreshTokenRaw = jwtService.GenerateRefreshTokenString();
            var refreshHash = jwtService.HashToken(refreshTokenRaw);

            // Decide refresh expiry from configuration (days)
            var refreshDays = int.Parse(configuration["Jwt:RefreshTokenDurationInDays"] ?? "30");
            var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshDays);

            // Persist refresh token
            var refreshToken = new RefreshToken
            {
                TokenHash = refreshHash,
                Expires = refreshExpiresAt,
                Created = DateTime.UtcNow,
                ApplicationUserId = user.Id
            };

            dbContext.RefreshTokens.Add(refreshToken);
            await dbContext.SaveChangesAsync();

            var tokenResponse = new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenRaw,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshTokenExpiresAt = refreshExpiresAt
            };

            var applicationUserDTO = new ApplicationUserDTO
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
            };

            return (tokenResponse, applicationUserDTO);
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(string refreshTokenRaw)
        {
            // Hash the provided refresh token and find in DB
            var providedHash = jwtService.HashToken(refreshTokenRaw);

            // Find the token in the DB and INCLUDE the User
            var storedToken = await dbContext.RefreshTokens
                .Include(t => t.ApplicationUser) // <--- Load the user here!
                .FirstOrDefaultAsync(t => t.TokenHash == providedHash);

            if (storedToken == null || !storedToken.IsActive)
                throw new BadRequestException("Invalid or expired refresh token.");
            var user = storedToken.ApplicationUser;

            // Revoke the old refresh token (rotate)
            storedToken.Revoked = DateTime.UtcNow;

            // Create new refresh token
            var newRefreshTokenRaw = jwtService.GenerateRefreshTokenString();
            var newRefreshHash = jwtService.HashToken(newRefreshTokenRaw);
            var refreshDays = int.Parse(configuration["Jwt:RefreshTokenDurationInDays"] ?? "30");
            var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshDays);

            var newRefreshToken = new RefreshToken
            {
                TokenHash = newRefreshHash,
                Expires = refreshExpiresAt,
                Created = DateTime.UtcNow,
                ApplicationUserId = user.Id
            };

            dbContext.RefreshTokens.Update(storedToken);
            dbContext.RefreshTokens.Add(newRefreshToken);

            // Generate new access token
            var newAccessToken = jwtService.GenerateAccessToken(user, out DateTime newAccessExpiresAt);

            await dbContext.SaveChangesAsync();

            return new TokenResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenRaw,
                AccessTokenExpiresAt = newAccessExpiresAt,
                RefreshTokenExpiresAt = refreshExpiresAt
            };
        }

        // Logout: revoke all refresh tokens for a user
        public async Task LogoutAsync(string userId)
        {
            var tokens = await dbContext.RefreshTokens.Where(t => t.ApplicationUserId == userId && t.Revoked == null).ToListAsync();
            if (!tokens.Any())
                return;

            foreach (var t in tokens)
                t.Revoked = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
        }

        public async Task SendVerificationAsync(ApplicationUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var verifyUrl = $"{frontendUrl}/verify-email?token={encodedToken}&email={user.Email}";
            await emailService.SendEmailAsync(user.Email!,  verifyUrl);
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                throw new BadRequestException("Invalid or expired token.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            return result.Succeeded;
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
            if (user == null)
                throw new BadRequestException("Invalid or expired token.");


            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));

            var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

            if(!result.Succeeded)
            {
                throw new BadRequestException("Invalid or expired token.");
            }
            await emailService.SendPasswordChangedNotification(user.Email!);
            return true;
        }

    }
}
