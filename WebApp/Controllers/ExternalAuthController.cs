using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WebApp.Data;
using WebApp.Entities;
using WebApp.RequiredAuthAtts;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers
{
    public class ExternalAuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExternalAuthController> _logger;

        public ExternalAuthController(ApplicationDbContext context, ILogger<ExternalAuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse"),
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme }
                }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Google authentication failed");
                return RedirectToAction("Login", "Auth");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var firstName = result.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = result.Principal.FindFirstValue(ClaimTypes.Surname);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    FirstName = firstName ?? name?.Split(' ')[0] ?? "User",
                    LastName = lastName ?? name?.Split(' ').Last() ?? "User",
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Projects");
        }

        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ExternalCallback()
        {
            return View();
        }
    }
}