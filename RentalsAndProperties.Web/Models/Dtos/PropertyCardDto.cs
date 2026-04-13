namespace RentalsAndProperties.Web.Models.Dtos
{
    public class PropertyCardDto
    {
        public Guid PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string ListingType { get; set; } = string.Empty;
        public string BHKType { get; set; } = string.Empty;
        public string FurnishingType { get; set; } = string.Empty;
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public double SquareFeet { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string OwnerName { get; set; } = string.Empty;
        public double AverageRating { get; set; }   // 0.0 when no reviews
        public int ReviewCount { get; set; }   // 0 when no reviews


    }
}