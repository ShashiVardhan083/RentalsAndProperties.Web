using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Mappings;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Report;

namespace RentalsAndProperties.Web.Controllers
{
    [JwtAuthorize]
    public class ReportController : Controller
    {
        private readonly ReportApiService ReportApi;
        private readonly PropertyApiService PropertyApi;

        public ReportController(ReportApiService reportApi, PropertyApiService propertyApi)
        {
            ReportApi = reportApi;
            PropertyApi = propertyApi;
        }

        [HttpGet]
        public async Task<IActionResult> Create(string targetType, Guid targetId, string? targetTitle = null)
        {
            if (string.IsNullOrWhiteSpace(targetType) || targetId == Guid.Empty)
            {
                TempData["Error"] = "Invalid report request.";
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine($"targetType: '{targetType}'");
            if (targetType == "Property" || targetType == "property")
            {
                var property = await PropertyApi.GetDetailsAsync(targetId);

                var userName = User.Identity?.Name;

                if (property?.Success == true &&
                       property.Data != null &&
                       !string.IsNullOrEmpty(userName) &&
                        userName.Equals(property.Data.OwnerName, StringComparison.OrdinalIgnoreCase)) 
                {
                    TempData["Error"] = "You cannot report your own property.";
                    return RedirectToAction("Details", "Property", new { id = targetId });
                }
            }
            return View(new CreateReportViewModel
            {
                TargetType = targetType,
                TargetId = targetId,
                TargetTitle = targetTitle
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReportViewModel createReportViewModel)
        {
            if (!ModelState.IsValid)
                return View(createReportViewModel);

            var result = await ReportApi.CreateReportAsync(
                createReportViewModel.TargetType,
                createReportViewModel.TargetId,
                createReportViewModel.TargetTitle,
                createReportViewModel.Reason,
                createReportViewModel.Description);

            if (result?.Success == true)
            {
                TempData["ToastSuccess"] = "Report submitted successfully.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = result?.Message ?? "Failed to submit report.";
            return View(createReportViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> MyReports()
        {
            var result = await ReportApi.GetMyReportsAsync();

            var reportResponseDtos = result?.Data ?? new List<ReportResponseDto>();
            var reportViewModel = ReportMapper.ToViewModel(reportResponseDtos);
            return View(reportViewModel);
        }
    }
}