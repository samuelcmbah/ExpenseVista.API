using ExpenseVista.API.DTOs.Auth;
using ExpenseVista.API.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseVista.API.Services.IServices
{
    public interface IAuthService
    {
        Task<(bool Succeeded, List<string> Errors)> RegisterAsync(RegisterDTO registerDTO);
        Task<(TokenResponseDTO tokenResponse, ApplicationUserDTO applicationUserDTO)> LoginAsync(LoginDTO dto);
        Task<(TokenResponseDTO tokenResponse, ApplicationUserDTO applicationUserDTO)> GoogleLoginAsync(GoogleLoginRequestDTO dto);
        Task<TokenResponseDTO> RefreshTokenAsync(string refreshTokenRaw);
        Task LogoutAsync(string userId);
        Task SendVerificationAsync(ApplicationUser user);
        Task<bool> ConfirmEmailAsync(string email, string token);

        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);
    }

}
