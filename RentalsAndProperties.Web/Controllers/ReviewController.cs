using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Mappings;
using RentalsAndProperties.Web.Models.Dtos;
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

            var userName = User.Identity?.Name;

            if (!string.IsNullOrEmpty(userName) &&
                userName.Equals(result.Data.OwnerName, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You cannot review your own property.";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }

            var createReviewViewModel = new CreateReviewViewModel
            {
                PropertyId = propertyId,
                PropertyTitle = result.Data.Title,
                ReviewType = reviewType,
                TransactionId = transactionId
            };

            return View(createReviewViewModel);
        }

        // POST /Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel createReviewViewModel)
        {
            if (!ModelState.IsValid)
                return View(createReviewViewModel);
            var CreateReviewRequestDto = ReviewMapper.ToCreateDto(createReviewViewModel);
            var result = await ReviewApi.CreateAsync(CreateReviewRequestDto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Failed to submit review.");
                return View(createReviewViewModel);
            }

            TempData["ToastSuccess"] = " Review submitted! Thank you for your feedback.";
            return RedirectToAction("Details", "Property", new { id = createReviewViewModel.PropertyId });
        }
    }
}