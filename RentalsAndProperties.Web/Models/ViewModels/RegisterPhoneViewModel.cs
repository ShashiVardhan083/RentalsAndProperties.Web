using System.ComponentModel.DataAnnotations;

namespace RentalsAndProperties.Web.Models.ViewModels
{
    public class RegisterPhoneViewModel
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(?:\+91|91)?[6-9]\d{9}$", ErrorMessage = "Enter a valid phone number. Eg: 9192999494")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
