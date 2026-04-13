using System.ComponentModel.DataAnnotations;

namespace RentalsAndProperties.Web.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?[1-9]\d{9,14}$",
            ErrorMessage = "Enter a valid phone number (e.g. +919876543210).")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

    }
}
