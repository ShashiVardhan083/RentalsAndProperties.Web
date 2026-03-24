using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Models.Enums;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Property;
using RentalsAndProperties.Web.ViewModels.Review;

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
        public async Task<IActionResult> Browse([FromQuery] PropertySearchQueryDto queryDto)
        {
            // Get cities
            var citiesResult = await SearchApi.GetCitiesAsync();
            var cities = citiesResult?.Data ?? new List<string>();

            // Call search API
            var result = await SearchApi.SearchAsync(queryDto);

            var queryVm = new PropertySearchQueryViewModel
            {
                City = queryDto.City,
                ListingType = queryDto.ListingType,
                PropertyType = queryDto.PropertyType,
                BHKType = queryDto.BHKType,
                MinPrice = queryDto.MinPrice,
                MaxPrice = queryDto.MaxPrice,
                Page = queryDto.Page,
                SortBy = queryDto.SortBy
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

                    Query = queryVm,
                    IsAuthenticated = SessionHelper.IsAuthenticated(HttpContext.Session)
                });
            }

            // Map Properties
            var properties = result.Data.Properties.Select(p => new PropertyCardViewModel
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                City = p.City,
                Price = p.Price,
                ListingType = p.ListingType,
                PropertyType = p.PropertyType,
                BHKType = p.BHKType,
                FurnishingType = p.FurnishingType,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                SquareFeet = p.SquareFeet,
                OwnerName = p.OwnerName,
                OwnerTrustScore = p.OwnerTrustScore,
                PrimaryImageUrl = p.PrimaryImageUrl,
                ImageUrls = p.ImageUrls,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount
            }).ToList();

            var vm = new PropertySearchResultViewModel
            {
                Properties = properties,
                TotalCount = result.Data.TotalCount,
                CurrentPage = result.Data.CurrentPage,
                TotalPages = result.Data.TotalPages,

                Query = queryVm,
                IsAuthenticated = SessionHelper.IsAuthenticated(HttpContext.Session)
            };

            vm.Cities = CityDetailsEnum.Data
                .Select(c => new SelectListItem
                {
                    Value = c.Key.ToString(),
                    Text = c.Value
                }).ToList();

            vm.ListingTypes = Enum.GetValues(typeof(ListingTypeEnum))
                .Cast<ListingTypeEnum>()
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = x.ToString()
                }).ToList();

            vm.BhkTypes = Enum.GetValues(typeof(BhkTypeEnum))
                .Cast<BhkTypeEnum>()
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = BhkLabelsEnum.Get(x)
                }).ToList();

            vm.FurnishingTypes = Enum.GetValues(typeof(FurnishingTypeEnum))
                .Cast<FurnishingTypeEnum>()
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = x.ToString()
                }).ToList();

            return View(vm);
        }

        // Property Detail 

        [HttpGet("Property/Details/{id:guid}")]
        [JwtAuthorize]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await PropertyApi.GetDetailsAsync(id);

            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = result?.Message ?? "Property not found.";
                return RedirectToAction(nameof(Index));
            }

            var p = result.Data;
            var reviewsResult = await ReviewApi.GetPropertyReviewsAsync(id);
            var reviews = reviewsResult?.Data ?? new List<ReviewViewModel>();

            ViewBag.Reviews = reviews;

            var vm = new PropertyDetailViewModel
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
                OwnerTrustScore = p.OwnerTrustScore,
                OwnerId = p.OwnerId,
                Images = p.Images.Select(i => new PropertyImageViewModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList()
            };

            return View(vm);
        }

        //  Owner Dashboard 

        [HttpGet]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var result = await PropertyApi.GetOwnerPropertiesAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load your properties.";
                return View(new List<PropertyCardViewModel>());
            }

            var vm = result.Data.Select(p => new PropertyCardViewModel
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                City = p.City,
                Price = p.Price,
                ListingType = p.ListingType,
                PropertyType = p.PropertyType,
                BHKType = p.BHKType,
                PrimaryImageUrl = p.PrimaryImageUrl,
                ImageUrls = p.ImageUrls ?? new List<string>(),
                Status = p.Status,  
                CreatedAt = p.CreatedAt,
            }).ToList();

            if (TempData.ContainsKey("Success"))
                ViewBag.Success = TempData["Success"];

            if (TempData.ContainsKey("Error") &&
                HttpContext.Request.Path.Value!.Contains("/Property/Dashboard"))
                ViewBag.Error = TempData["Error"];

            return View(vm);
        }

        //  Create Property 

        [HttpGet]
        [JwtAuthorize("Owner", "Admin")]
        public IActionResult Create()
        {
            SetEnumViewBags();
            return View(new CreatePropertyViewModel { AvailableFrom = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Create(CreatePropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetEnumViewBags();
                return View(model);
            }

            if (model.Images != null)
            {
                var invalid = model.Images.Where(f =>
                    f.Length > 5 * 1024 * 1024 ||
                    !new[] { ".jpg", ".jpeg", ".png", ".webp" }
                        .Contains(Path.GetExtension(f.FileName).ToLower())).ToList();

                if (invalid.Any())
                {
                    ModelState.AddModelError("Images",
                        "One or more images exceed 5 MB or have an unsupported format.");
                    SetEnumViewBags();
                    return View(model);
                }
            }

            var result = await PropertyApi.CreatePropertyAsync(model);

            if (result == null || !result.Success)
            {
                foreach (var err in result?.Errors ?? new List<string>())
                    ModelState.AddModelError(string.Empty, err);

                if (!ModelState.Values.SelectMany(v => v.Errors).Any())
                    ModelState.AddModelError(string.Empty,
                        result?.Message ?? "Failed to create property.");

                SetEnumViewBags();
                return View(model);
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
            var model = new EditPropertyViewModel
            {
                PropertyId = detail.PropertyId,
                Title = detail.Title,
                Description = detail.Description,
                City = Enum.TryParse<CityEnum>(detail.City, true, out var city) ? city : CityEnum.Hyderabad,
                Address = detail.Address,
                Price = detail.Price,
                SecurityDeposit = detail.SecurityDeposit,
                SquareFeet = detail.SquareFeet,
                Bedrooms = detail.Bedrooms,
                Bathrooms = detail.Bathrooms,
                AvailableFrom = detail.AvailableFrom,
                CurrentStatus = detail.Status,
                ExistingImages = detail.Images.Select(i => new PropertyImageViewModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList()
            };

            if (Enum.TryParse<PropertyTypeEnum>(detail.PropertyType, true, out var pt))
                model.PropertyType = pt;
            if (Enum.TryParse<ListingTypeEnum>(detail.ListingType, true, out var lt))
                model.ListingType = lt;
            if (Enum.TryParse<FurnishingTypeEnum>(detail.FurnishingType, true, out var ft))
                model.FurnishingType = ft;

            var cityMatch = Enum.GetValues<CityEnum>()
                .FirstOrDefault(c => CityDetailsEnum.GetName(c)
                    .Equals(detail.City, StringComparison.OrdinalIgnoreCase));
            model.City = cityMatch;
            model.Pincode = detail.Pincode ?? "";

            model.BHKType = detail.BHKType switch
            {
                "1 RK" => BhkTypeEnum.OneRK,
                "1 BHK" => BhkTypeEnum.OneBHK,
                "2 BHK" => BhkTypeEnum.TwoBHK,
                "3 BHK" => BhkTypeEnum.ThreeBHK,
                "4 BHK" => BhkTypeEnum.FourBHK,
                "Penthouse" => BhkTypeEnum.Penthouse,
                _ => BhkTypeEnum.OneBHK
            };

            SetEnumViewBags();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Edit(Guid id, EditPropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var detail = await PropertyApi.GetDetailsAsync(id);
                model.ExistingImages = detail?.Data?.Images.Select(i => new PropertyImageViewModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                }).ToList() ?? new List<PropertyImageViewModel>();

                SetEnumViewBags();
                return View(model);
            }

            var result = await PropertyApi.UpdatePropertyAsync(id, model);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Update failed.");
                SetEnumViewBags();
                return View(model);
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

        //  Helpers 

        private void SetEnumViewBags()
        {
            ViewBag.PropertyTypes = Enum.GetValues<PropertyTypeEnum>();
            ViewBag.ListingTypes = Enum.GetValues<ListingTypeEnum>();
            ViewBag.BHKTypes = Enum.GetValues<BhkTypeEnum>();
            ViewBag.FurnishingTypes = Enum.GetValues<FurnishingTypeEnum>();
            ViewBag.Cities = Enum.GetValues<CityEnum>()
                .Select(c => new { Value = c.ToString(), Name = CityDetailsEnum.GetName(c) })
                .ToList();
        }
    }
}