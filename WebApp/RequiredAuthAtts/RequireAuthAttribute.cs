using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.RequiredAuthAtts
{
    public class RequireAuthAttribute : TypeFilterAttribute
    {
        public RequireAuthAttribute() : base(typeof(RequireAuthFilter))
        {
        }

        private class RequireAuthFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var userId = context.HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                }
            }
        }
    }
}