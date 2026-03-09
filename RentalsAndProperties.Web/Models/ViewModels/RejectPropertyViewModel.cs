using System.ComponentModel.DataAnnotations;

namespace RentalsAndProperties.Web.Models.ViewModels
{
    public class RejectPropertyViewModel
    {
        public Guid PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a rejection reason.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be 10–500 characters.")]
        [Display(Name = "Rejection Reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
