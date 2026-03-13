using System.ComponentModel.DataAnnotations;

namespace RentalsAndProperties.Web.ViewModels.Transaction
{
    public class TransactionViewModel
    {
        // Transaction details
        public Guid TransactionId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentStatus { get; set; } = "";

        public string TransactionStatus { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public bool CustomerConfirmed { get; set; }

        public bool OwnerConfirmed { get; set; }

        public bool HasReview { get; set; }

        public string CustomerName { get; set; } = "";

        // Property details
        public Guid PropertyId { get; set; }

        public string PropertyTitle { get; set; } = string.Empty;

        public string PropertyCity { get; set; } = string.Empty;

        public decimal PropertyPrice { get; set; }

        public string PropertyType { get; set; } = string.Empty;

        public string ListingType { get; set; } = string.Empty;

        public string BHKType { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public double OwnerTrustScore { get; set; }

        // Transaction input
        [Required(ErrorMessage = "Transaction type is required.")]
        public string TransactionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment method is required.")]
        public string PaymentMethod { get; set; } = "Online";
    }
}