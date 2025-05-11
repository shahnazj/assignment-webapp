using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace WebApp.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email", Prompt = "Enter email address")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Prompt = "Enter password")]
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }
}