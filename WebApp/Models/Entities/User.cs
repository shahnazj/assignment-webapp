using System;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        public string? PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}