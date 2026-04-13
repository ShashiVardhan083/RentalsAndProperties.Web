using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.ViewModels.Transaction;

namespace RentalsAndProperties.Web.Mappings
{
    public static class TransactionMapper
    {
        // ViewModel -> DTO (POST)
        public static CreateTransactionRequestDto ToCreateDto(TransactionViewModel transactionViewModel)
        {
            return new CreateTransactionRequestDto
            {
                PropertyId = transactionViewModel.PropertyId,
                TransactionType = transactionViewModel.TransactionType,
                PaymentMethod = transactionViewModel.PaymentMethod
            };
        }

        // DTO -> ViewModel (MyTransactions)
        public static List<TransactionViewModel> ToViewModel(List<TransactionResponseDto> transactionResponseDtoList)
        {
            return transactionResponseDtoList.Select(t => new TransactionViewModel
            {
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

                PropertyId = t.PropertyId,
                PropertyTitle = t.PropertyTitle,
                PropertyCity = t.PropertyCity,
                OwnerName = t.OwnerName,
                CustomerId = t.CustomerId,
                OwnerId = t.OwnerId,
                PropertyPrice = t.Amount,
                TransactionType = t.TransactionType
            }).ToList();
        }
    }
}