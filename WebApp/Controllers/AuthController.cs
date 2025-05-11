using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;
using WebApp.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller responsible for handling user authentication operations
    /// </summary>
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string DefaultAuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        private const string GoogleAuthenticationScheme = GoogleDefaults.AuthenticationScheme;
        private const string UserIdSessionKey = "UserId";
        private const string UserEmailSessionKey = "UserEmail";
        private const string UserNameSessionKey = "UserName";

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the sign up page
        /// </summary>
        [HttpGet]
        [Route("auth/signup")]
        public IActionResult SignUp()
        {
            return View();
        }

        /// <summary>
        /// Handles user registration
        /// </summary>
        [HttpPost]
        [Route("auth/signup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email address is already registered");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account created successfully! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Displays the login page
        /// </summary>
        [HttpGet]
        [Route("auth/login")]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Handles user login
        /// </summary>
        [HttpPost]
        [Route("auth/login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            await UpdateUserLastLogin(user);
            await SignInUser(user, model.RememberMe);

            return RedirectToAction("Index", "Projects", new { area = "Admin" });
        }

        /// <summary>
        /// Handles user logout
        /// </summary>
        [HttpGet]
        [Route("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(DefaultAuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Initiates Google authentication
        /// </summary>
        [HttpGet]
        [Route("auth/google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleResponse)) };
            return Challenge(properties, GoogleAuthenticationScheme);
        }

        /// <summary>
        /// Handles Google authentication response
        /// </summary>
        [HttpGet]
        [Route("auth/google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleAuthenticationScheme);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Google authentication failed.";
                return RedirectToAction(nameof(Login));
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var firstName = result.Principal.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = result.Principal.FindFirst(ClaimTypes.Surname)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Could not get email from Google account.";
                return RedirectToAction(nameof(Login));
            }

            var user = await GetOrCreateGoogleUser(email, firstName, lastName, name);
            await SignInUser(user, true);

            return RedirectToAction("Index", "Projects", new { area = "Admin" });
        }

        #region Private Methods

        private async Task<User> GetOrCreateGoogleUser(string email, string firstName, string lastName, string name)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    FirstName = firstName ?? name?.Split(' ')[0] ?? "Google",
                    LastName = lastName ?? name?.Split(' ').Last() ?? "User",
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
            }
            else
            {
                user.LastLoginAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return user;
        }

        private async Task UpdateUserLastLogin(User user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private async Task SignInUser(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                DefaultAuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetString(UserIdSessionKey, user.Id.ToString());
            HttpContext.Session.SetString(UserEmailSessionKey, user.Email);
            HttpContext.Session.SetString(UserNameSessionKey, $"{user.FirstName} {user.LastName}");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        #endregion
    }
}