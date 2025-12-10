using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVista.API.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        // Store a hashed token, not raw token
        [Required]
        public string TokenHash { get; set; } = null!;

        [Required]
        public DateTime Expires { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Revoked { get; set; }

        // Foreign key to user
        [Required]
        public string ApplicationUserId { get; set; } = null!;

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; } = null!;
        /// <summary>
        /// A computed property acting as a gatekeeper for a refreshtoken to be considered usable.
        /// Returns true if the token is not revoked and is not expired
        /// </summary>
        public bool IsActive => Revoked == null && DateTime.UtcNow <= Expires;
    }
}
