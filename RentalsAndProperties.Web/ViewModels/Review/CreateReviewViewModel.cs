using System.ComponentModel.DataAnnotations;

namespace RentalsAndProperties.Web.ViewModels.Review
{
    public class CreateReviewViewModel
    {
        public Guid PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;

        public string ReviewType { get; set; } = "Interaction";
        public Guid? TransactionId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Comment { get; set; } = string.Empty;

        public int OwnerResponsiveness { get; set; }
        public int PropertyAccuracy { get; set; }
    }
}
