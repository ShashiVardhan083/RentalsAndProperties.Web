using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Review;

namespace RentalsAndProperties.Web.Controllers
{
    [JwtAuthorize]
    public class ReviewController : Controller
    {
        private readonly ReviewApiService ReviewApi;
        private readonly PropertyApiService PropertyApi;

        public ReviewController(ReviewApiService reviewApi, PropertyApiService propertyApi)
        {
            ReviewApi = reviewApi;
            PropertyApi = propertyApi;
        }

        // GET /Review/Create?propertyId=&reviewType=Interaction
        // GET /Review/Create?propertyId=&reviewType=Transaction&transactionId=
        [HttpGet]
        public async Task<IActionResult> Create(Guid propertyId, string reviewType = "Interaction",
                                                 Guid? transactionId = null)
        {
            var result = await PropertyApi.GetDetailsAsync(propertyId);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = "Property not found.";
                return RedirectToAction("Browse", "Property");
            }

            var vm = new ReviewViewModel
            {
                PropertyId = propertyId,
                PropertyTitle = result.Data.Title,
                ReviewType = reviewType,
                TransactionId = transactionId
            };

            return View(vm);
        }

        // POST /Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await ReviewApi.CreateAsync(vm);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Failed to submit review.");
                return View(vm);
            }

            TempData["ToastSuccess"] = " Review submitted! Thank you for your feedback.";
            return RedirectToAction("Details", "Property", new { id = vm.PropertyId });
        }
    }
}