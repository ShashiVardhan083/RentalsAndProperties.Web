namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class PropertyCardViewModel
    {
        public Guid PropertyId { get; set; }

        public string Title { get; set; } = "";

        public string City { get; set; } = "";

        public decimal Price { get; set; }

        public string ListingType { get; set; } = "";
        public string PropertyType { get; set; } = "";

        public string BHKType { get; set; } = "";

        public string FurnishingType { get; set; } = "";

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public int SquareFeet { get; set; }

        public string? PrimaryImageUrl { get; set; }

        public List<string> ImageUrls { get; set; } = new();

        public int ImageCount => ImageUrls?.Count ?? 0;

        public string OwnerName { get; set; } = "";

        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public double OwnerTrustScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }
}