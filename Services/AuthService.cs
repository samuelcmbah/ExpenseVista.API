using ExpenseVista.API.Common.Exceptions;
using ExpenseVista.API.Configurations;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Auth;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.IServices;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

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
        private readonly IHttpClientFactory httpClientFactory;
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
            IHttpClientFactory httpClientFactory,
            ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            this.jwtService = jwtService;
            this.normalizer = normalizer;
            this.logger = logger;
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            this.dbContext = dbContext;
            frontendUrl = appOptions.Value.FrontendUrl;
        }

        // --- PRIVATE HELPER METHODS ---

        private async Task<ApplicationUser> FindOrCreateUserFromGooglePayloadAsync(GoogleJsonWebSignature.Payload payload)
        {
            // 1. Find user by Google ID (standard login)
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.ProviderName == "Google" && u.ProviderKey == payload.Subject);
            if (user != null) return user;

            // 2. Find user by email (account linking)
            user = await userManager.FindByEmailAsync(payload.Email);
            if (user != null)
            {
                // This is the linking step
                user.ProviderName = "Google";
                user.ProviderKey = payload.Subject;
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
                return user;
            }

            // 3. Create a new user (first-time registration with Google)
            user = new ApplicationUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                FirstName = payload.GivenName ?? "",
                LastName = payload.FamilyName ?? "",
                EmailConfirmed = true, // Email is verified by Google
                ProviderName = "Google",
                ProviderKey = payload.Subject
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return user;
        }

        // NEW REFACTORED helper method for generating tokens
        private async Task<TokenResponseDTO> GenerateAndPersistTokensAsync(ApplicationUser user)
        {
            var accessToken = jwtService.GenerateAccessToken(user, out DateTime accessExpiresAt);
            var refreshTokenRaw = jwtService.GenerateRefreshTokenString();
            var refreshHash = jwtService.HashToken(refreshTokenRaw);

            var refreshDays = int.Parse(configuration["Jwt:RefreshTokenDurationInDays"] ?? "30");
            var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshDays);

            var refreshToken = new RefreshToken
            {
                TokenHash = refreshHash,
                Expires = refreshExpiresAt,
                Created = DateTime.UtcNow,
                ApplicationUserId = user.Id
            };

            dbContext.RefreshTokens.Add(refreshToken);
            await dbContext.SaveChangesAsync();

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenRaw,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshTokenExpiresAt = refreshExpiresAt
            };
        }

        private async Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(string code)
        {
            var clientId = configuration["Google:ClientId"];
            var clientSecret = configuration["Google:ClientSecret"];
            // This must be the SAME URI you registered in Google Cloud Console
            var redirectUri = "http://localhost:5000/auth/google/callback";

            var tokenRequest = new
            {
                code,
                client_id = clientId,
                client_secret = clientSecret,
                redirect_uri = redirectUri,
                grant_type = "authorization_code"
            };

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://oauth2.googleapis.com/token", tokenRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Google token exchange failed: {errorContent}");
            }

            var tokens = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
            return tokens ?? throw new ApplicationException("Failed to deserialize Google token response.");
        }

        // Private record to hold Google's response
        private record GoogleTokenResponse(
            [property: JsonPropertyName("access_token")] string AccessToken,
            [property: JsonPropertyName("id_token")] string IdToken
        );

        // --- PUBLIC SERVICE METHODS --

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
            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (!user.EmailConfirmed)
            {
                throw new UnauthorizedAccessException("EMAIL_NOT_CONFIRMED");
            }


            var tokenResponse = await GenerateAndPersistTokensAsync(user);


            var applicationUserDTO = new ApplicationUserDTO
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
            };

            return (tokenResponse, applicationUserDTO);
        }

        public async Task<(TokenResponseDTO tokenResponse, ApplicationUserDTO applicationUserDTO)> GoogleLoginAsync(GoogleLoginRequestDTO dto)
        {
            // 1. Exchange the Authorization Code for Google's tokens
            var googleTokens = await ExchangeCodeForTokensAsync(dto.AuthorizationCode);

            // 2. Validate Google's ID token and get user info
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleTokens.IdToken);

            // 3. Find or Create the user in your database (with account linking)
            var user = await FindOrCreateUserFromGooglePayloadAsync(payload);

            // 4. Generate YOUR OWN JWTs for the user
            var tokenResponse = await GenerateAndPersistTokensAsync(user);

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
            {
                throw new BadRequestException("Invalid or expired token.");
            }

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
