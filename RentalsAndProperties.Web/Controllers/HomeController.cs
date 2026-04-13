using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Constants;
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
            // Load approved properties for the featured section
            var query = new PropertySearchQueryDto
            {
                Page = PaginationConstants.pageNo,
                PageSize = PaginationConstants.pageSize,
                SortBy = PaginationConstants.sortBy
            };

            var result = await SearchApi.SearchAsync(query);

            var propertyListViewModel = new PropertyListViewModel
            {
                TotalCount = result?.Data?.TotalCount ?? 0,

                Properties = result?.Data?.Properties?.Select(property => new PropertyCardViewModel
                {
                    PropertyId = property.PropertyId,
                    Title = property.Title,
                    City = property.City,
                    Price = property.Price,
                    ListingType = property.ListingType,
                    PropertyType = property.PropertyType,
                    BHKType = property.BHKType,
                    PrimaryImageUrl = property.PrimaryImageUrl
                }).ToList() ?? new(),

                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,

                FullName = User.Identity?.Name ?? string.Empty,

                Roles = User.Claims
               .Where(c => c.Type == ClaimTypes.Role)
               .Select(c => c.Value)
               .ToList()
            };
            return View(propertyListViewModel);
        }

        [HttpGet("Home/Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id
                            ?? HttpContext.TraceIdentifier
            });
        }
    }
}