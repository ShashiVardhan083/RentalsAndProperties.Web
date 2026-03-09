using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RentalsAndProperties.Web.Filters
{
    // Custom authorization filter
    // Checks session for JwtToken. If absent, redirects to login.
    // Optional role parameter enforces role-based access.
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
            var session = context.HttpContext.Session;
            var token = session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                // Not authenticated – redirect to login with return URL
                var returnUrl = Uri.EscapeDataString(
                    context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);

                context.Result = new RedirectToActionResult(
                    "Login", "Auth", new { returnUrl });
                return;
            }

            // Role check
            if (RequiredRoles.Length > 0)
            {
                var rolesJson = session.GetString("UserRoles") ?? "[]";
                var userRoles = System.Text.Json.JsonSerializer.Deserialize<List<string>>(rolesJson)
                                ?? new List<string>();

                var hasRole = RequiredRoles.Any(r => userRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
                if (!hasRole)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                }
            }
        }
    }
}