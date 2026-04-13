using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Mappings;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Models.Enums;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Property;

namespace RentalsAndProperties.Web.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyApiService PropertyApi;
        private readonly PropertySearchApiService SearchApi;
        private readonly ILogger<PropertyController> Logger;
        private readonly ReviewApiService ReviewApi;
        private readonly LocationApiService LocationApi;

        public PropertyController(
            LocationApiService locationApi,
            PropertyApiService propertyApi,
            PropertySearchApiService searchApi,
            ReviewApiService reviewApi,
            ILogger<PropertyController> logger)
        {
            LocationApi = locationApi;
            PropertyApi = propertyApi;
            SearchApi = searchApi;
            ReviewApi = reviewApi;
            Logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPincode(string city)
        {
            var pincode = await LocationApi.GetPincodeAsync(city);
            return Json(new { pincode });
        }


        // Search & Filter 

        [HttpGet]
        public async Task<IActionResult> Browse([FromQuery] PropertySearchQueryDto propertySearchQueryDto)
        {
            var result = await SearchApi.SearchAsync(propertySearchQueryDto);

            var propertySearchQueryViewModel = new PropertySearchQueryViewModel
            {
                City = propertySearchQueryDto.City,
                ListingType = propertySearchQueryDto.ListingType,
                PropertyType = propertySearchQueryDto.PropertyType,
                BHKType = propertySearchQueryDto.BHKType,
                MinPrice = propertySearchQueryDto.MinPrice,
                MaxPrice = propertySearchQueryDto.MaxPrice,
                Page = propertySearchQueryDto.Page,
                SortBy = propertySearchQueryDto.SortBy
            };

            // Handle error / empty response
            if (result?.Data == null)
            {
                return View(new PropertySearchResultViewModel
                {
                    Properties = new List<PropertyCardViewModel>(),
                    TotalCount = 0,
                    CurrentPage = 1,
                    TotalPages = 1,

                    Query = propertySearchQueryViewModel,
                    IsAuthenticated = User.Identity?.IsAuthenticated == true
                });
            }

            var properties = PropertyMapper.ToCardViewModel(result.Data.Properties ?? new());
            var propertySearchResultViewModel = new PropertySearchResultViewModel
            {
                Properties = properties,
                TotalCount = result.Data.TotalCount,
                CurrentPage = result.Data.CurrentPage,
                TotalPages = result.Data.TotalPages,

                Query = propertySearchQueryViewModel,
                IsAuthenticated = User.Identity?.IsAuthenticated == true
            };

            propertySearchResultViewModel.Cities = Enum.GetValues(typeof(CityEnum))
                .Cast<CityEnum>()
                .Select(c => new SelectListItem
                {
                    Value = ((int)c).ToString(),
                    Text = c.ToString()
                }).ToList();

            propertySearchResultViewModel.ListingTypes = Enum.GetValues(typeof(ListingTypeEnum))
                .Cast<ListingTypeEnum>()
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = x.ToString()
                }).ToList();

            propertySearchResultViewModel.BhkTypes = Enum.GetValues(typeof(BhkTypeEnum))
                .Cast<BhkTypeEnum>()
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = BhkLabelsEnum.Get(x)
                }).ToList();

            propertySearchResultViewModel.FurnishingTypes = Enum.GetValues(typeof(FurnishingTypeEnum))
                .Cast<FurnishingTypeEnum>()
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = x.ToString()
                }).ToList();

            return View(propertySearchResultViewModel);
        }

        // Property Detail 

       
        [JwtAuthorize]
        public async Task<IActionResult> Details(Guid id)
        {
            if (TempData["Success"] is string success) ViewBag.Success = success;
            if (TempData["Error"] is string error) ViewBag.Error = error;

            var result = await PropertyApi.GetDetailsAsync(id);

            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = result?.Message ?? "Property not found.";
                return RedirectToAction(nameof(Browse));
            }

            var p = result.Data;
            var reviewsResult = await ReviewApi.GetPropertyReviewsAsync(id);
            var reviews = reviewsResult?.Data ?? new List<ReviewResponseDto>();
            var propertyDetailViewModel = PropertyMapper.ToDetailViewModel(p, reviews);
            return View(propertyDetailViewModel);
        }

        //  Owner Dashboard 

        [HttpGet]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            if (TempData["Success"] is string success) ViewBag.Success = success;
            if (TempData["Error"] is string error) ViewBag.Error = error;

            var result = await PropertyApi.GetOwnerPropertiesAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load your properties.";
                return View(new List<PropertyCardViewModel>());
            }
            var propertyCardViewModel = PropertyMapper.ToOwnerCardViewModel(result.Data);
            return View(propertyCardViewModel);
        }

        //  Create Property 

        [HttpGet]
        [JwtAuthorize("Owner", "Admin")]
        public IActionResult Create()
        {
            return View(new CreatePropertyViewModel { AvailableFrom = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Create(CreatePropertyViewModel createPropertyViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(createPropertyViewModel);
            }

            if (createPropertyViewModel.Images != null)
            {
                var invalid = createPropertyViewModel.Images.Where(f =>                                                 
                    f.Length > 5 * 1024 * 1024 ||
                    !new[] { ".jpg", ".jpeg", ".png", ".webp" }
                        .Contains(Path.GetExtension(f.FileName).ToLower())).ToList();

                if (invalid.Any())
                {
                    ModelState.AddModelError("Images",
                        "One or more images exceed 5 MB or have an unsupported format.");
                    return View(createPropertyViewModel);
                }
            }

            var result = await PropertyApi.CreatePropertyAsync(createPropertyViewModel);

            if (result == null || !result.Success)
            {
                foreach (var err in result?.Errors ?? new List<string>())
                    ModelState.AddModelError(string.Empty, err);

                if (!ModelState.Values.SelectMany(v => v.Errors).Any())
                    ModelState.AddModelError(string.Empty,
                        result?.Message ?? "Failed to create property.");
                return View(createPropertyViewModel);
            }

            TempData["Success"] =
                "Property submitted for approval! Our team will review it within 24 hours.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Edit Property 

        [HttpGet]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await PropertyApi.GetDetailsAsync(id);

            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = "Property not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var detail = result.Data;
            var editPropertyViewModel = PropertyMapper.ToEditViewModel(result.Data);
            return View(editPropertyViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Edit(Guid id, EditPropertyViewModel editPropertyViewModel)
        {
            if (!ModelState.IsValid)
            {
                var detail = await PropertyApi.GetDetailsAsync(id);
                editPropertyViewModel.ExistingImages = detail?.Data?.Images.Select(i => new PropertyImageViewModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                }).ToList() ?? new List<PropertyImageViewModel>();

                return View(editPropertyViewModel);
            }

            var result = await PropertyApi.UpdatePropertyAsync(id, editPropertyViewModel);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Update failed.");
                return View(editPropertyViewModel);
            }

            TempData["Success"] = "Property updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }   

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await PropertyApi.DeletePropertyAsync(id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Property deleted successfully."
                    : (result?.Message ?? "Delete failed.");

            return RedirectToAction(nameof(Dashboard));
        }

    }
}