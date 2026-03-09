using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.ViewModels;
using RentalsAndProperties.Web.Services;

namespace RentalsAndProperties.Web.Controllers
{
    [JwtAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly PropertyApiService PropertyApi;
        private readonly ILogger<AdminController> Logger;

        public AdminController(PropertyApiService propertyApi, ILogger<AdminController> logger)
        {
            PropertyApi = propertyApi;
            Logger = logger;
        }

        // GET /Admin  – Pending properties dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await PropertyApi.GetPendingAsync();

            if (result == null || !result.Success)
            {
                ViewBag.Error = result?.Message ?? "Failed to load pending properties.";
                return View(new List<PropertyDetailModel>());
            }

            if (TempData["Success"] is string success) ViewBag.Success = success;
            if (TempData["Error"] is string error) ViewBag.Error = error;

            return View(result.Data ?? new List<PropertyDetailModel>());
        }

        // POST /Admin/Approve/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await PropertyApi.ApprovePropertyAsync(id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? $"✅ Property '{result.Data?.Title}' approved and is now live!"
                    : (result?.Message ?? "Approval failed.");

            return RedirectToAction(nameof(Index));
        }

        // GET /Admin/Reject/{id}  – Shows the reject form
        [HttpGet]
        public async Task<IActionResult> Reject(Guid id)
        {
            var result = await PropertyApi.GetDetailsAsync(id);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = "Property not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(new RejectPropertyViewModel
            {
                PropertyId = id,
                PropertyTitle = result.Data.Title
            });
        }

        // POST /Admin/Reject/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, RejectPropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await PropertyApi.RejectPropertyAsync(id, model.Reason);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? $"Property rejected. Owner will be notified."
                    : (result?.Message ?? "Rejection failed.");

            return RedirectToAction(nameof(Index));
        }
    }
}