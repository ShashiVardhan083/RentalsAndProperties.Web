using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.Models.ViewModels;
using RentalsAndProperties.Web.Services;

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
            if (SessionHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await AuthApi.LoginAsync(model.PhoneNumber, model.Password);

            if (result == null || !result.Success || result.Data == null)
            {
                var errorMsg = result?.Errors?.FirstOrDefault()
                               ?? result?.Message
                               ?? "Login failed. Please try again.";

                ModelState.AddModelError(string.Empty, errorMsg);
                return View(model);
            }

            // Store auth in session
            SessionHelper.SetAuthSession(
                HttpContext.Session,
                result.Data.Token,
                result.Data.FullName,
                result.Data.PhoneNumber,
                result.Data.Email,
                result.Data.Roles,
                result.Data.ExpiresAt);

            Logger.LogInformation("User logged in: {Phone}", model.PhoneNumber[..^4] + "****");

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
            if (SessionHelper.IsAuthenticated(HttpContext.Session))
                return RedirectToAction("Index", "Home");

            return View(new RegisterPhoneViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterPhoneViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await AuthApi.SendOtpAsync(model.PhoneNumber);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError(string.Empty,
                    result?.Message ?? "Failed to send OTP. Please try again.");
                return View(model);
            }

            // Store phone in TempData for step 2
            TempData["RegisterPhone"] = model.PhoneNumber;
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
        public async Task<IActionResult> VerifyOtp(OtpVerifyViewModel model)
        {
            ViewBag.DevOtp = TempData.Peek("DevOtp");

            if (!ModelState.IsValid)
                return View(model);

            var result = await AuthApi.VerifyOtpAsync(
                model.PhoneNumber,
                model.OtpCode,
                model.FullName,
                model.Email,
                model.Password,
                model.ConfirmPassword);

            if (result == null || !result.Success || result.Data == null)
            {
                var errors = result?.Errors ?? new List<string>();
                if (errors.Any())
                    foreach (var err in errors)
                        ModelState.AddModelError(string.Empty, err);
                else
                    ModelState.AddModelError(string.Empty, result?.Message ?? "Verification failed.");

                return View(model);
            }

            // Auto-login after registration
            SessionHelper.SetAuthSession(
                HttpContext.Session,
                result.Data.Token,
                result.Data.FullName,
                result.Data.PhoneNumber,
                result.Data.Email,
                result.Data.Roles,
                result.Data.ExpiresAt);

            TempData["WelcomeMessage"] = $"Welcome, {result.Data.FullName}! Your account has been created.";
            return RedirectToAction("Index", "Home");
        }

        //  Logout 

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize]
        public async Task<IActionResult> Logout()
        {
            var token = SessionHelper.GetToken(HttpContext.Session);
            if (!string.IsNullOrEmpty(token))
                await AuthApi.LogoutAsync(token);

            SessionHelper.ClearAuthSession(HttpContext.Session);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize]                   // must be logged in
        public async Task<IActionResult> BecomeOwner()
        {
            var result = await AuthApi.BecomeOwnerAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = result?.Message ?? "Something went wrong. Please try again.";
                return RedirectToAction("Index", "Home");
            }

            // Swap the stored JWT and refresh session roles
            SessionHelper.SetAuthSession(
                HttpContext.Session,
                result.Data.Token,
                result.Data.FullName,
                result.Data.PhoneNumber,
                result.Data.Email,
                result.Data.Roles,
                result.Data.ExpiresAt
            );

            TempData["Success"] =
                " You're now an Owner! Start listing your properties.";

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