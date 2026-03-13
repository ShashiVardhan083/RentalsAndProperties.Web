using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Property;

namespace RentalsAndProperties.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly PropertySearchApiService SearchApi;

        public HomeController(PropertySearchApiService searchApi)
        {
            SearchApi = searchApi;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.IsAuthenticated = SessionHelper.IsAuthenticated(HttpContext.Session);
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Roles = SessionHelper.GetRoles(HttpContext.Session);

            // Load approved properties for the featured section
            var query = new PropertySearchQueryDto
            {
                Page = 1,
                PageSize = 6,
                SortBy = "Newest"
            };

            var result = await SearchApi.SearchAsync(query);

            var vm = new PropertyListViewModel
            {
                TotalCount = result?.Data?.TotalCount ?? 0,
                Properties = result?.Data?.Properties?.Select(p => new PropertyCardViewModel
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    City = p.City,
                    Price = p.Price,
                    ListingType = p.ListingType,
                    PropertyType = p.PropertyType,
                    BHKType = p.BHKType,
                    PrimaryImageUrl = p.PrimaryImageUrl
                }).ToList() ?? new()
            };
            return View(vm);
        }

        [HttpGet("Home/Error")]
        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id
                            ?? HttpContext.TraceIdentifier
            });
        }
    }
}