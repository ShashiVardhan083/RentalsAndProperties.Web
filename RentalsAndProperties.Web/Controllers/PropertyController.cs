using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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
        private readonly PropertyMediaApiService MediaApi;
        private readonly PropertySearchApiService SearchApi;
        private readonly ILogger<PropertyController> Logger;
        private readonly ReviewApiService ReviewApi;
        private readonly LocationApiService LocationApi;

        public PropertyController(
            LocationApiService locationApi,
            PropertyApiService propertyApi,
            PropertyMediaApiService mediaApi,
            PropertySearchApiService searchApi,
            ReviewApiService reviewApi,
            ILogger<PropertyController> logger)
        {
            LocationApi = locationApi;
            PropertyApi = propertyApi;
            MediaApi = mediaApi;
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

        // Public: Approved Listings

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int page = 1)
        {
            var result = await PropertyApi.GetApprovedAsync(page, 12);

            if (result?.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load properties.";
                return View(new PropertyListViewModel());
            }

            var vm = new PropertyListViewModel
            {
                CurrentPage = result.Data.Page,
                TotalPages = result.Data.TotalPages,
                TotalCount = result.Data.TotalCount,
                Properties = result.Data.Items.Select(p => new PropertyCardViewModel
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    City = p.City,
                    Price = p.Price,
                    ListingType = p.ListingType,
                    PropertyType = p.PropertyType,
                    BHKType = p.BHKType,
                    PrimaryImageUrl = p.PrimaryImageUrl
                }).ToList()
            };

            return View(vm);
        }

        // Search & Filter 

        [HttpGet]
        public async Task<IActionResult> Browse([FromQuery] PropertySearchQueryDto query)
        {
            var citiesResult = await SearchApi.GetCitiesAsync();
            ViewBag.Cities = citiesResult?.Data ?? new List<string>();

            var result = await SearchApi.SearchAsync(query);

            ViewBag.IsAuthenticated = SessionHelper.IsAuthenticated(HttpContext.Session);
            ViewBag.Query = query;

            if (result?.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Search failed.";
                return View(new PropertySearchResultViewModel());
            }

            var vm = new PropertySearchResultViewModel
            {
                TotalCount = result.Data.TotalCount,
                CurrentPage = result.Data.CurrentPage,
                TotalPages = result.Data.TotalPages,

                Properties = result.Data.Properties.Select(p => new PropertyCardViewModel
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
                }).ToList()
            };

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

            /*    var vm = result.Data.Select(p => new PropertyCardViewModel
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    City = p.City,
                    Price = p.Price,
                    ListingType = p.ListingType,
                    PropertyType = p.PropertyType,
                    BHKType = p.BHKType,
                    PrimaryImageUrl = p.PrimaryImageUrl
                }).ToList();
            */
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
                //  Add this line:
                ImageUrls = p.ImageUrls ?? new List<string>(),
                Status = p.Status,           // ✅ Also add Status so status pills work
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
                City = Enum.TryParse<CityWeb>(detail.City, true, out var city) ? city : CityWeb.Hyderabad,
                Address = detail.Address,
                Price = detail.Price,
                SecurityDeposit = detail.SecurityDeposit,
                SquareFeet = detail.SquareFeet,
                Bedrooms = detail.Bedrooms,
                Bathrooms = detail.Bathrooms,
                AvailableFrom = detail.AvailableFrom,
                CurrentStatus = detail.Status,
                ExistingImages = detail.Images.Select(i => new PropertyImageDto
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList()
            };

            if (Enum.TryParse<PropertyTypeWeb>(detail.PropertyType, true, out var pt))
                model.PropertyType = pt;
            if (Enum.TryParse<ListingTypeWeb>(detail.ListingType, true, out var lt))
                model.ListingType = lt;
            if (Enum.TryParse<FurnishingTypeWeb>(detail.FurnishingType, true, out var ft))
                model.FurnishingType = ft;

            var cityMatch = Enum.GetValues<CityWeb>()
                .FirstOrDefault(c => CityDetails.GetName(c)
                    .Equals(detail.City, StringComparison.OrdinalIgnoreCase));
            model.City = cityMatch;
            model.Pincode = detail.Pincode ?? "";

            model.BHKType = detail.BHKType switch
            {
                "1 RK" => BHKTypeWeb.OneRK,
                "1 BHK" => BHKTypeWeb.OneBHK,
                "2 BHK" => BHKTypeWeb.TwoBHK,
                "3 BHK" => BHKTypeWeb.ThreeBHK,
                "4 BHK" => BHKTypeWeb.FourBHK,
                "Penthouse" => BHKTypeWeb.Penthouse,
                _ => BHKTypeWeb.OneBHK
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
                model.ExistingImages = detail?.Data?.Images.Select(i => new PropertyImageDto
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                }).ToList() ?? new List<PropertyImageDto>();

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

        //  Upload Images (GET) 

        [HttpGet]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> UploadImages(Guid id)
        {
            var detail = await PropertyApi.GetDetailsAsync(id);

            if (detail == null || !detail.Success || detail.Data == null)
            {
                TempData["Error"] = "Property not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var vm = new UploadImagesViewModel
            {
                PropertyId = id,
                PropertyTitle = detail.Data.Title,
                ExistingImages = detail.Data.Images.Select(i => new PropertyImageDto
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList()
            };

            return View(vm);
        }

        // Upload Images (POST) 

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> UploadImages(Guid id, UploadImagesViewModel model)
        {
            // Validate: files must be selected
            if (model.Images == null || model.Images.Count == 0)
            {
                TempData["Error"] = "Please select at least one image to upload.";
                return RedirectToAction(nameof(UploadImages), new { id });
            }

            // Client-side validation before sending to API
            var invalidFiles = model.Images.Where(f =>
                f.Length == 0 ||
                f.Length > 5 * 1024 * 1024 ||
                !new[] { ".jpg", ".jpeg", ".png", ".webp" }
                    .Contains(Path.GetExtension(f.FileName).ToLower())).ToList();

            if (invalidFiles.Any())
            {
                TempData["Error"] = "One or more files are invalid. Max 5MB each. Allowed: JPG, PNG, WebP.";
                return RedirectToAction(nameof(UploadImages), new { id });
            }

            var result = await MediaApi.UploadImagesAsync(id, model.Images);

            if (result == null || !result.Success)
            {
                TempData["Error"] = result?.Message ?? "Upload failed. Please try again.";
                return RedirectToAction(nameof(UploadImages), new { id });
            }

            TempData["Success"] = $"{result.Data?.Count ?? 0} image(s) uploaded successfully.";
            return RedirectToAction(nameof(UploadImages), new { id });
        }

        // Delete Image 

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> DeleteImage(Guid imageId, Guid propertyId)
        {
            var result = await MediaApi.DeleteImageAsync(imageId);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Image deleted successfully."
                    : (result?.Message ?? "Delete failed.");

            return RedirectToAction(nameof(UploadImages), new { id = propertyId });
        }

        // Set Primary Image 

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> SetPrimaryImage(Guid imageId, Guid propertyId)
        {
            var result = await MediaApi.SetPrimaryAsync(imageId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = result?.Success ?? false, message = result?.Message });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Primary image updated."
                    : (result?.Message ?? "Failed to set primary image.");

            return RedirectToAction(nameof(UploadImages), new { id = propertyId });
        }

        //  Delete Property 

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
            ViewBag.PropertyTypes = Enum.GetValues<PropertyTypeWeb>();
            ViewBag.ListingTypes = Enum.GetValues<ListingTypeWeb>();
            ViewBag.BHKTypes = Enum.GetValues<BHKTypeWeb>();
            ViewBag.FurnishingTypes = Enum.GetValues<FurnishingTypeWeb>();
            ViewBag.Cities = Enum.GetValues<CityWeb>()
                .Select(c => new { Value = c.ToString(), Name = CityDetails.GetName(c) })
                .ToList();
        }
    }
}