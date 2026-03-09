using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.Services;

namespace RentalsAndProperties.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly PropertyApiService PropertyApi;

        public HomeController(PropertyApiService propertyApi)
        {
            PropertyApi = propertyApi;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.IsAuthenticated = SessionHelper.IsAuthenticated(HttpContext.Session);
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Roles = SessionHelper.GetRoles(HttpContext.Session);

            // Load approved properties for the featured section
            var result = await PropertyApi.GetApprovedAsync(1, 6);
            ViewBag.Properties = result?.Data;

            return View();
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