using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.ViewModels.Auth;

namespace RentalsAndProperties.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiService AuthApi;
        private readonly ILogger<AuthController> Logger;

        public AuthController(AuthApiService authApi, ILogger<AuthController> logger)
        {
            AuthApi = authApi;
            Logger = logger;
        }


        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(loginViewModel);

            var result = await AuthApi.LoginAsync(loginViewModel.PhoneNumber, loginViewModel.Password);

            if (result == null || !result.Success || result.Data == null)
            {
                var errorMsg = result?.Errors?.FirstOrDefault()
                               ?? result?.Message
                               ?? "Login failed. Please try again.";

                ModelState.AddModelError(string.Empty, errorMsg);
                return View(loginViewModel);
            }

            await AuthHelper.SignInUser(
                HttpContext,
                result.Data.FullName,
                result.Data.Email!,
                result.Data.Token,
                result.Data.Roles
            );
            Logger.LogInformation("User logged in: {Phone}", loginViewModel.PhoneNumber[..^4] + "****");

            TempData["ToastSuccess"] = $"Welcome back, {result.Data.FullName}!";

            // Redirect admin to admin dashboard
            if (result.Data.Roles != null && result.Data.Roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction("Index", "Home");
        }

        //  Registration – Step 1: Phone 

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new RegisterPhoneViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterPhoneViewModel registerPhoneViewModel)
        {
            if (!ModelState.IsValid)
                return View(registerPhoneViewModel);

            var result = await AuthApi.SendOtpAsync(registerPhoneViewModel.PhoneNumber);

            if (result == null || !result.Success)
            {
                var errorMsg =
                    result?.Errors?.FirstOrDefault()
                    ?? result?.Message
                    ?? "Failed to send OTP. Please try again.";

                ModelState.AddModelError(nameof(registerPhoneViewModel.PhoneNumber), errorMsg);
                ModelState.AddModelError(string.Empty, errorMsg);

                return View(registerPhoneViewModel);
            }

            // Store phone in TempData for step 2
            TempData["RegisterPhone"] = registerPhoneViewModel.PhoneNumber;
            TempData["DevOtp"] = result.Data?.DevOtp; // dev mode only

            return RedirectToAction(nameof(VerifyOtp));
        }

        // Registration – Step 2: OTP + Profile 

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var phone = TempData.Peek("RegisterPhone") as string;
            if (string.IsNullOrEmpty(phone))
                return RedirectToAction(nameof(Register));

            ViewBag.DevOtp = TempData.Peek("DevOtp");

            return View(new OtpVerifyViewModel { PhoneNumber = phone });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpVerifyViewModel verifyOtpViewModel)
        {
            ViewBag.DevOtp = TempData.Peek("DevOtp");

            if (!ModelState.IsValid)
                return View(verifyOtpViewModel);

            var result = await AuthApi.VerifyOtpAsync(
                verifyOtpViewModel.PhoneNumber,
                verifyOtpViewModel.OtpCode,
                verifyOtpViewModel.FullName,
                verifyOtpViewModel.Email,
                verifyOtpViewModel.Password,
                verifyOtpViewModel.ConfirmPassword);

            if (result == null || !result.Success || result.Data == null)
            {
                var errors = result?.Errors ?? new List<string>();
                if (errors.Any())
                    foreach (var err in errors)
                        ModelState.AddModelError(string.Empty, err);
                else
                    ModelState.AddModelError(string.Empty, result?.Message ?? "Verification failed.");

                return View(verifyOtpViewModel);
            }

            var token = User.Claims
                .FirstOrDefault(c => c.Type == "JwtToken")?.Value;

            if (!string.IsNullOrEmpty(token))
                await AuthApi.LogoutAsync(token);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await AuthHelper.SignInUser(
               HttpContext,
               result.Data.FullName,
               result.Data.Email!,
               result.Data.Token,
               result.Data.Roles
           );

            TempData["WelcomeMessage"] = $"Welcome, {result.Data.FullName}! Your account has been created.";
            return RedirectToAction("Index", "Home");
        }

        //  Logout 

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize]
        public async Task<IActionResult> Logout()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;

            if (!string.IsNullOrEmpty(token))
                await AuthApi.LogoutAsync(token);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize]
        public async Task<IActionResult> BecomeOwner()
        {
            var result = await AuthApi.BecomeOwnerAsync();

            Logger.LogInformation("BecomeOwner Success: {success}", result?.Success);
            Logger.LogInformation("BecomeOwner Message: {message}", result?.Message);
            Logger.LogInformation("BecomeOwner Data Null?: {dataNull}", result?.Data == null);

            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = result?.Message ?? "Something went wrong. Please try again.";
                // Redirect back to wherever they came from
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
                    return Redirect(referer);
                return RedirectToAction("Index", "Home");
            }

            // Refresh session with new token + roles
            await AuthHelper.SignInUser(
               HttpContext,
               result.Data.FullName,
               result.Data.Email!,
               result.Data.Token,
               result.Data.Roles
           );
            TempData["Success"] = " You're now an Owner! Start listing your properties.";
            return RedirectToAction("Dashboard", "Property");
        }
        // Access Denied

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}