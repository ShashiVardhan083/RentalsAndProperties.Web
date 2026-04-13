using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace RentalsAndProperties.Web.Filters
{
    //Is user logged in?
//Does user have a JWT?
//Does user have required roles?
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class JwtAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] RequiredRoles;

        public JwtAuthorizeAttribute(params string[] roles)
        {
            RequiredRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check authentication using COOKIE (Claims)
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                var returnUrl = Uri.EscapeDataString(
                    context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);

                context.Result = new RedirectToActionResult(
                    "Login", "Auth", new { returnUrl });

                return;
            }

            // Check JWT existence (from claims)
            var token = user.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Role check using claims
            if (RequiredRoles.Length > 0)
            {
                var hasRole = RequiredRoles.Any(role =>
                    user.Claims.Any(c =>
                        c.Type == ClaimTypes.Role &&
                        c.Value.Equals(role, StringComparison.OrdinalIgnoreCase)));

                if (!hasRole)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                }
            }
        }
    }
}