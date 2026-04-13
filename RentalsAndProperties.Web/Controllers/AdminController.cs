using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Mappings;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Admin;
using RentalsAndProperties.Web.ViewModels.Property;

namespace RentalsAndProperties.Web.Controllers
{
    [JwtAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly PropertyApiService PropertyApi;
        private readonly ReportApiService ReportApi;
        private readonly AnalyticsApiService AnalyticsApi;
        private readonly ILogger<AdminController> Logger;

        public AdminController(PropertyApiService propertyApi, ILogger<AdminController> logger, ReportApiService reportApi, AnalyticsApiService analyticsApi)
        {
            PropertyApi = propertyApi;
            Logger = logger;
            ReportApi = reportApi;
            AnalyticsApi = analyticsApi;
        }

        // GET /Admin  – Pending properties dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            if (TempData["Success"] is string success) ViewBag.Success = success;
            if (TempData["Error"] is string error) ViewBag.Error = error;

            var result = await PropertyApi.GetPendingAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load pending properties.";
                return View(new List<PropertyDetailViewModel>());
            }

            var propertyDetailViewModel = PropertyMapper.ToViewModel(result.Data);

            return View(propertyDetailViewModel);
        }

        // POST /Admin/Approve/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRejectProperty(Guid id, string? reason, bool isApproved)
        {
            var result = await PropertyApi.GetDetailsAsync(id);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = "Property not found.";
                return RedirectToAction(nameof(Index));
            }

            await PropertyApi.ApproveRejectPropertyAsync(id, reason, isApproved);
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


        //  Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var result = await AnalyticsApi.GetAnalyticsAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Analytics service failed.";
                Logger.LogError("Analytics error: {msg}", result?.Message);
                return View(new AdminAnalyticsViewModel());
            }

            var adminAnalyticsDto = result.Data;
            var adminAnalyticsViewModel = AnalyticsMapper.ToViewModel(result.Data);
            return View(adminAnalyticsViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AllUsers()
        {
            var result = await AnalyticsApi.GetAllUsersAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load users.";
                return View(new List<UserListViewModel>());
            }
            var userListViewModel = UserMapper.ToViewModel(result.Data);
            return View(userListViewModel);
        }

        //  Reports list
        [HttpGet]
        public async Task<IActionResult> Reports([FromQuery] string? filter = null)
        {
            var result = await ReportApi.GetAllReportsAsync(filter);

            ViewBag.Filter = filter;

            var reportResponseDtos = result?.Data ?? new List<ReportResponseDto>();
            var reportViewModel = ReportMapper.ToViewModel(reportResponseDtos);
            return View(reportViewModel);
        }

        //  Resolve report
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport(Guid id)
        {
            var result = await ReportApi.ResolveReportAsync(id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Report resolved." : (result?.Message ?? "Failed.");
            return RedirectToAction(nameof(Reports));
        }

        //  Reject report
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReport(Guid id)
        {
            var result = await ReportApi.RejectReportAsync(id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Report rejected." : (result?.Message ?? "Failed.");
            return RedirectToAction(nameof(Reports));
        }
    }
}