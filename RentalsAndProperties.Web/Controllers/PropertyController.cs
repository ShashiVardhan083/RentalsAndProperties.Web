using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Enums;
using RentalsAndProperties.Web.Models.ViewModels;
using RentalsAndProperties.Web.Services;

namespace RentalsAndProperties.Web.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyApiService PropertyApi;
        private readonly PropertyMediaApiService MediaApi;
        private readonly PropertySearchApiService SearchApi;
        private readonly ILogger<PropertyController> Logger;

        public PropertyController(
            PropertyApiService propertyApi,
            PropertyMediaApiService mediaApi,
            PropertySearchApiService searchApi,
            ILogger<PropertyController> logger)
        {
            PropertyApi = propertyApi;
            MediaApi = mediaApi;
            SearchApi = searchApi;
            Logger = logger;
        }

        // Public: Approved Listings Grid
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int page = 1)
        {
            var result = await PropertyApi.GetApprovedAsync(page, 12);

            if (result?.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load properties.";
                return View(new PagedResultModel<PropertyResponseModel>());
            }

            ViewBag.IsAuthenticated = SessionHelper.IsAuthenticated(HttpContext.Session);
            return View(result.Data);
        }

        // Phase 4: Search & Filter 
        [HttpGet]
        public async Task<IActionResult> Browse([FromQuery] PropertySearchQueryModel query)
        {
            // Fetch cities for the filter dropdown
            var citiesResult = await SearchApi.GetCitiesAsync();
            ViewBag.Cities = citiesResult?.Data ?? new List<string>();

            // Run search
            var result = await SearchApi.SearchAsync(query);

            ViewBag.IsAuthenticated = SessionHelper.IsAuthenticated(HttpContext.Session);
            ViewBag.Query = query;

            if (result?.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Search failed. Please try again.";
                return View(new PropertySearchResultModel());
            }

            return View(result.Data);
        }

        //  Property Detail
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

            ViewBag.IsOwner = result.Data.OwnerId.ToString() ==
                              HttpContext.Session.GetString("UserId");

            return View(result.Data);
        }

        // Owner Dashboard
        [HttpGet]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var result = await PropertyApi.GetOwnerPropertiesAsync();

            if (result == null || !result.Success)
            {
                ViewBag.Error = result?.Message ?? "Failed to load your properties.";
                return View(new List<PropertyResponseModel>());
            }

            if (TempData["Success"] is string s) ViewBag.Success = s;
            if (TempData["Error"] is string e) ViewBag.Error = e;

            return View(result.Data ?? new List<PropertyResponseModel>());
        }

        // Create Property
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
                City = detail.City,
                Address = detail.Address,
                Price = detail.Price,
                SecurityDeposit = detail.SecurityDeposit,
                SquareFeet = detail.SquareFeet,
                Bedrooms = detail.Bedrooms,
                Bathrooms = detail.Bathrooms,
                AvailableFrom = detail.AvailableFrom,
                CurrentStatus = detail.Status,
                ExistingImages = detail.Images.Select(i => new PropertyImageModel
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
                model.ExistingImages = detail?.Data?.Images.Select(i => new PropertyImageModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList() ?? new List<PropertyImageModel>();

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

        // ── Phase 3: Upload Images ───────────────────────────────────────────
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
                ExistingImages = detail.Data.Images.Select(i => new PropertyImageModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> UploadImages(Guid id, UploadImagesViewModel model)
        {
            if (model.Images == null || model.Images.Count == 0)
            {
                TempData["Error"] = "Please select at least one image to upload.";
                return RedirectToAction(nameof(UploadImages), new { id });
            }

            var result = await MediaApi.UploadImagesAsync(id, model.Images);

            if (result == null || !result.Success)
            {
                TempData["Error"] = result?.Message ?? "Upload failed.";
                return RedirectToAction(nameof(UploadImages), new { id });
            }

            TempData["Success"] = $"{result.Data?.Count ?? 0} image(s) uploaded successfully.";
            return RedirectToAction(nameof(UploadImages), new { id });
        }

        // ── Phase 3: Delete Image (AJAX) ─────────────────────────────────────
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

        // ── Phase 3: Set Primary Image (AJAX) ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [JwtAuthorize("Owner", "Admin")]
        public async Task<IActionResult> SetPrimaryImage(Guid imageId, Guid propertyId)
        {
            var result = await MediaApi.SetPrimaryAsync(imageId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result?.Success ?? false, message = result?.Message });
            }

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Primary image updated."
                    : (result?.Message ?? "Failed to set primary image.");

            return RedirectToAction(nameof(UploadImages), new { id = propertyId });
        }

        // ── Delete Property ──────────────────────────────────────────────────
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

        // ── Helpers ──────────────────────────────────────────────────────────
        private void SetEnumViewBags()
        {
            ViewBag.PropertyTypes = Enum.GetValues<PropertyTypeWeb>();
            ViewBag.ListingTypes = Enum.GetValues<ListingTypeWeb>();
            ViewBag.BHKTypes = Enum.GetValues<BHKTypeWeb>();
            ViewBag.FurnishingTypes = Enum.GetValues<FurnishingTypeWeb>();
        }
    }
}