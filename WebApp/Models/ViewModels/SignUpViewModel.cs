using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace WebApp.Models
{


    public class SignUpViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First Name", Prompt = "Enter first name")]
        public string FirstName { get; set; } = null!;

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name", Prompt = "Enter last name")]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email", Prompt = "Enter email address")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Prompt = "Enter password")]
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password", Prompt = "Confirm password")]
        public string ConfirmPassword { get; set; } = null!;

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        [Display(Name = "Accept Terms")]
        public bool Terms { get; set; }
    }
}