using System.ComponentModel.DataAnnotations;

namespace RentalsAndProperties.Web.ViewModels.Review
{
    public class ReviewViewModel
    {
        public Guid PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string ReviewType { get; set; } = "Interaction";   // "Interaction" | "Transaction"
        public Guid? TransactionId { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Comment must be at least 10 characters.")]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5)]
        public int OwnerResponsiveness { get; set; } = 5;

        [Range(1, 5)]
        public int PropertyAccuracy { get; set; } = 5;

        [Range(1, 5)]
        public int? PriceSatisfaction { get; set; }

        public bool? WouldRecommend { get; set; }
        public Guid ReviewId { get; set; }

        public string ReviewerName { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
