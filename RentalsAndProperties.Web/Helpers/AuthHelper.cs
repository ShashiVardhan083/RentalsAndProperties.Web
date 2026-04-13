using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace RentalsAndProperties.Web.Helpers
{
    public static class AuthHelper
    {
        public static async Task SignInUser(
            HttpContext httpContext,
            string fullName,
            string email,
            string token,
            List<string>? roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, fullName ?? ""),
                new Claim(ClaimTypes.Email, email ?? ""),
                new Claim("JwtToken", token)
            };

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );
        }
    }
}