using Microsoft.AspNetCore.Identity;

namespace ExpenseVista.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // public string? FullName { get; set; }
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
