using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using WebApp.Models.Consent;

namespace WebApp.Controllers
{
    [Route("cookies")]
    public class CookiesController : Controller
    {
        [HttpPost("setcookies")]
        public IActionResult SetCookies([FromBody] CookieConsent consent)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("cookieConsent", System.Text.Json.JsonSerializer.Serialize(consent), cookieOptions);
            return Ok();
        }

        [HttpGet("check-cookie-consent")]
        public IActionResult CheckCookieConsent()
        {
            var consent = Request.Cookies["cookieConsent"];
            return Json(new { hasConsent = !string.IsNullOrEmpty(consent) });
        }
    }
}