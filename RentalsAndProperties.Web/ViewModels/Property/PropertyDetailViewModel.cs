using RentalsAndProperties.Web.ViewModels.Review;

namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class PropertyDetailViewModel
    {
        public Guid PropertyId { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public string City { get; set; } = "";

        public string Address { get; set; } = "";

        public string Pincode { get; set; } = "";

        public decimal Price { get; set; }

        public decimal? SecurityDeposit { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public int SquareFeet { get; set; }

        public string PropertyType { get; set; } = "";

        public string ListingType { get; set; } = "";

        public string BHKType { get; set; } = "";

        public string FurnishingType { get; set; } = "";

        public DateTime AvailableFrom { get; set; }

        public string Status { get; set; } = "";
        public string OwnerName { get; set; } = "";

        public string OwnerPhone { get; set; } = "";
        public Guid OwnerId { get; set; }

        public DateTime CreatedAt { get; set; }
        public List<ReviewDisplayViewModel> Reviews { get; set; } = new();
        public List<PropertyImageViewModel> Images { get; set; } = new();

        public string? PrimaryImageUrl =>
            Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
            ?? Images.FirstOrDefault()?.ImageUrl;

        public List<string> AllImageUrls =>
            Images.OrderByDescending(i => i.IsPrimary)
                  .Select(i => i.ImageUrl)
                  .ToList();
    }
}