using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Report;
using RentalsAndProperties.Web.Models.Dtos;

namespace RentalsAndProperties.Web.Controllers
{
    [JwtAuthorize]
    public class ReportController : Controller
    {
        private readonly ReportApiService ReportApi;

        public ReportController(ReportApiService reportApi)
        {
            ReportApi = reportApi;
        }

        [HttpGet]
        public IActionResult Create(string targetType, Guid targetId, string? targetTitle = null)
        {
            return View(new CreateReportViewModel
            {
                TargetType = targetType,
                TargetId = targetId,
                TargetTitle = targetTitle
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await ReportApi.CreateReportAsync(
                model.TargetType,
                model.TargetId,
                model.Reason,
                model.Description);

            if (result?.Success == true)
            {
                TempData["ToastSuccess"] = "Report submitted successfully.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = result?.Message ?? "Failed to submit report.";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyReports()
        {
            var result = await ReportApi.GetMyReportsAsync();

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
    }
}