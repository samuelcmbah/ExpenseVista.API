using ExpenseVista.API.DTOs.Auth;
using ExpenseVista.API.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseVista.API.Services.IServices
{
    public interface IAuthService
    {
        Task<(bool Succeeded, List<string> Errors)> RegisterAsync(RegisterDTO registerDTO);
        Task SendVerificationAsync(ApplicationUser user);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<(string Token, ApplicationUserDTO UserDTO)> LoginAsync(LoginDTO dto);

        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);
    }

}
