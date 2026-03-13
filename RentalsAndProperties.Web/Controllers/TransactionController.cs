using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Helpers;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Transaction;

namespace RentalsAndProperties.Web.Controllers
{
    [JwtAuthorize]
    public class TransactionController : Controller
    {
        private readonly TransactionApiService TransactionApi;
        private readonly PropertyApiService PropertyApi;

        public TransactionController(TransactionApiService transactionApi, PropertyApiService propertyApi)
        {
            TransactionApi = transactionApi;
            PropertyApi = propertyApi;
        }

        // GET /Transaction/Initiate/{propertyId}
        [HttpGet]
        public async Task<IActionResult> Initiate(Guid propertyId)
        {
            var result = await PropertyApi.GetDetailsAsync(propertyId);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData["Error"] = "Property not found.";
                return RedirectToAction("Browse", "Property");
            }

            var prop = result.Data;

            // Owner cannot transact on their own property
            var userId = SessionHelper.GetPhone(HttpContext.Session); // not used for check here, done server side
            var sessionRoles = SessionHelper.GetRoles(HttpContext.Session);

            var vm = new TransactionViewModel
            {
                PropertyId = prop.PropertyId,
                PropertyTitle = prop.Title,
                PropertyCity = prop.City,
                PropertyPrice = prop.Price,
                PropertyType = prop.PropertyType,
                ListingType = prop.ListingType,
                BHKType = prop.BHKType,
                OwnerName = prop.OwnerName,
                OwnerTrustScore = prop.OwnerTrustScore,
                TransactionType = prop.ListingType == "Sale" ? "Sale" : "Rent"
            };

            return View(vm);
        }

        // POST /Transaction/Initiate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate(TransactionViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await TransactionApi.CreateAsync(vm);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Transaction failed. Please try again.");
                return View(vm);
            }

            TempData["ToastSuccess"] = vm.PaymentMethod == "Online"
                ? "🎉 Payment successful! Transaction completed."
                : "✅ Transaction initiated. Waiting for owner confirmation.";

            return RedirectToAction(nameof(MyTransactions));
        }

        // GET /Transaction/MyTransactions
        [HttpGet]
        public async Task<IActionResult> MyTransactions()
        {
            var result = await TransactionApi.GetMyTransactionsAsync();

            if (TempData["ToastSuccess"] is string msg)
                ViewBag.Success = msg;

            if (result == null || !result.Success || result.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load transactions.";
                return View(new List<TransactionViewModel>());
            }

            var vm = result.Data.Select(t => new TransactionViewModel
            {
                // Transaction details
                TransactionId = t.TransactionId,
                Amount = t.Amount,
                PaymentStatus = t.PaymentStatus,
                TransactionStatus = t.TransactionStatus,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt,
                CustomerConfirmed = t.CustomerConfirmed,
                OwnerConfirmed = t.OwnerConfirmed,
                HasReview = t.HasReview,
                CustomerName = t.CustomerName,

                // Property details
                PropertyId = t.PropertyId,
                PropertyTitle = t.PropertyTitle,
                PropertyCity = t.PropertyCity,
                OwnerName = t.OwnerName,

                // Derived values (because DTO doesn't contain them)
                PropertyPrice = t.Amount,
                TransactionType = t.TransactionType
            }).ToList();

            return View(vm);
        }

        // POST /Transaction/Confirm/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var result = await TransactionApi.ConfirmAsync(id);

            TempData["ToastSuccess"] = result?.Success == true
                ? "✅ Confirmed! Transaction updated."
                : (result?.Message ?? "Confirmation failed.");

            return RedirectToAction(nameof(MyTransactions));
        }
    }
}