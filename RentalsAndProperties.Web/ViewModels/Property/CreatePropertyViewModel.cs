using System.ComponentModel.DataAnnotations;
using RentalsAndProperties.Web.Models.Enums;

namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class CreatePropertyViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be 5-150 characters.")]
        [Display(Name = "Property Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "Description must be 20-2000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [Display(Name = "City")]
        public CityEnum? City { get; set; }

        [Display(Name = "Pincode")]
        public string Pincode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, 99999999)]
        [Display(Name = "Price")]
        public decimal? Price { get; set; }

        [Range(0, 99999999)]
        [Display(Name = "Security Deposit")]
        public decimal? SecurityDeposit { get; set; }

        [Required(ErrorMessage = "Square feet is required.")]
        [Range(10, 100000)]
        [Display(Name = "Area (sq.ft)")]
        public double SquareFeet { get; set; }

        [Required]
        [Range(0, 20)]
        [Display(Name = "Bedrooms")]
        public int Bedrooms { get; set; }

        [Required]
        [Range(0, 20)]
        [Display(Name = "Bathrooms")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "Select property type.")]
        [Display(Name = "Property Type")]
        public PropertyTypeEnum? PropertyType { get; set; }

        [Required(ErrorMessage = "Select listing type.")]
        [Display(Name = "Listing Type")]
        public ListingTypeEnum? ListingType { get; set; }

        [Required(ErrorMessage = "Select BHK type.")]
        [Display(Name = "BHK Configuration")]
        public BhkTypeEnum? BHKType { get; set; }

        [Required(ErrorMessage = "Select furnishing status.")]
        [Display(Name = "Furnishing")]
        public FurnishingTypeEnum? FurnishingType { get; set; }

        [Required(ErrorMessage = "Available from date is required.")]
        [Display(Name = "Available From")]
        public DateTime AvailableFrom { get; set; } = DateTime.Today;

        [Display(Name = "Property Images")]
        public List<IFormFile>? Images { get; set; }
    }
}
