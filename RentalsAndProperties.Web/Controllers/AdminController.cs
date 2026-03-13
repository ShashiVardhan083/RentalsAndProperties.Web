using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Admin;
using RentalsAndProperties.Web.ViewModels.Property;
using RentalsAndProperties.Web.ViewModels.Report;

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
            var result = await PropertyApi.GetPendingAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load pending properties.";
                return View(new List<PropertyDetailViewModel>());
            }

            if (TempData["Success"] is string success) ViewBag.Success = success;
            if (TempData["Error"] is string error) ViewBag.Error = error;

            var vm = result.Data.Select(p => new PropertyDetailViewModel
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                Description = p.Description,
                City = p.City,
                Address = p.Address,
                Pincode = p.Pincode,
                Price = p.Price,
                SecurityDeposit = p.SecurityDeposit,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                SquareFeet = (int)p.SquareFeet,
                PropertyType = p.PropertyType,
                ListingType = p.ListingType,
                BHKType = p.BHKType,
                FurnishingType = p.FurnishingType,
                AvailableFrom = p.AvailableFrom,
                Status = p.Status,
                OwnerName = p.OwnerName,
                OwnerPhone = p.OwnerPhone,
                OwnerTrustScore = p.OwnerTrustScore
            }).ToList();

            return View(vm);
        }

        // POST /Admin/Approve/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await PropertyApi.ApprovePropertyAsync(id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? $"Property '{result.Data?.Title}' approved and is now live!"
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

            var dto = result.Data;

            var vm = new AdminAnalyticsViewModel
            {
                TotalUsers = dto.TotalUsers,
                TotalOwners = dto.TotalOwners,
                TotalProperties = dto.TotalProperties,
                PendingApprovals = dto.PendingApprovals,
                ActiveListings = dto.ActiveListings,
                CompletedTransactions = dto.CompletedTransactions,
                TotalReviews = dto.TotalReviews,
                TotalReports = dto.TotalReports,
                AveragePropertyPrice = dto.AveragePropertyPrice,

                TopCities = dto.TopCities.Select(c => new CityStatViewModel
                {
                    City = c.City,
                    Count = c.Count
                }).ToList(),

                MonthlyRegistrations = dto.MonthlyRegistrations.Select(m => new MonthlyStatViewModel
                {
                    Month = m.Month,
                    Count = m.Count
                }).ToList(),

                MonthlyTransactions = dto.MonthlyTransactions.Select(m => new MonthlyStatViewModel
                {
                    Month = m.Month,
                    Count = m.Count
                }).ToList(),

                RecentReports = dto.RecentReports.Select(r => new ReportViewModel
                {
                    ReportId = r.ReportId,
                    ReporterName = r.ReporterName,
                    TargetType = r.TargetType,
                    TargetId = r.TargetId,
                    Reason = r.Reason,
                    Description = r.Description,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ResolvedAt = r.ResolvedAt
                }).ToList()
            };

            return View(vm);
        }

        //  Reports list
        [HttpGet]
        public async Task<IActionResult> Reports([FromQuery] string? filter = null)
        {
            var result = await ReportApi.GetAllReportsAsync(filter);

            ViewBag.Filter = filter;

            var dtos = result?.Data ?? new List<ReportResponseDto>();

            var vm = dtos.Select(r => new ReportViewModel
            {
                ReportId = r.ReportId,
                ReporterName = r.ReporterName,
                TargetType = r.TargetType,
                TargetId = r.TargetId,
                Reason = r.Reason,
                Description = r.Description,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ResolvedAt = r.ResolvedAt
            }).ToList();

            return View(vm);
        }

        //  Resolve report
        [HttpPost("Reports/Resolve/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport(Guid id)
        {
            var result = await ReportApi.ResolveReportAsync(id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Report resolved." : (result?.Message ?? "Failed.");
            return RedirectToAction(nameof(Reports));
        }

        //  Reject report
        [HttpPost("Reports/Reject/{id:guid}")]
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