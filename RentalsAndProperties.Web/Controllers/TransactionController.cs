using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Mappings;
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
            var property = result.Data;

            // Owner cannot transact on their own property
            var userName = User.Identity?.Name;


            if (!string.IsNullOrEmpty(userName) &&
                    userName.Equals(property.OwnerName, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You cannot initiate a transaction on your own property.";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }

            var transactionViewModel = new TransactionViewModel
            {
                PropertyId = property.PropertyId,
                PropertyTitle = property.Title,
                PropertyCity = property.City,
                PropertyPrice = property.Price,
                PropertyType = property.PropertyType,
                ListingType = property.ListingType,
                BHKType = property.BHKType,
                OwnerName = property.OwnerName,
                TransactionType = property.ListingType == "Sale" ? "Sale" : "Rent"
            };

            return View(transactionViewModel);
        }

        // POST /Transaction/Initiate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate(TransactionViewModel transactionViewModel)
        {

            if (!ModelState.IsValid)
                return View(transactionViewModel);

            var createTransactionRequestDto = TransactionMapper.ToCreateDto(transactionViewModel);
            var result = await TransactionApi.CreateAsync(createTransactionRequestDto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Transaction failed. Please try again.");
                return View(transactionViewModel);
            }

            TempData["ToastSuccess"] = transactionViewModel.PaymentMethod == "Online"
                ? " Payment successful! Transaction completed."
                : " Transaction initiated. Waiting for owner confirmation.";

            return RedirectToAction(nameof(MyTransactions));
        }

        // GET /Transaction/MyTransactions
        [HttpGet]
        public async Task<IActionResult> MyTransactions()
        {
            var result = await TransactionApi.GetMyTransactionsAsync();

            if (result == null || !result.Success || result.Data == null)
            {
                ViewBag.Error = result?.Message ?? "Failed to load transactions.";
                return View(new List<TransactionViewModel>());
            }

            var transactionViewModel = TransactionMapper.ToViewModel(result.Data);
            return View(transactionViewModel);
        }

        // POST /Transaction/Confirm/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var result = await TransactionApi.ConfirmAsync(id);

            TempData["ToastSuccess"] = result?.Success == true
                ? " Confirmed! Transaction updated."
                : (result?.Message ?? "Confirmation failed.");

            return RedirectToAction(nameof(MyTransactions));
        }
    }
}