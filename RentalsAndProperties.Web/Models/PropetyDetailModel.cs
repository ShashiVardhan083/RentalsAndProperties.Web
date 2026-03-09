namespace RentalsAndProperties.Web.Models
{
    public class PropertyDetailModel
    {
        public Guid PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? SecurityDeposit { get; set; }
        public double SquareFeet { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string ListingType { get; set; } = string.Empty;
        public string BHKType { get; set; } = string.Empty;
        public string FurnishingType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime AvailableFrom { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public double OwnerTrustScore { get; set; }
        public List<PropertyImageModel> Images { get; set; } = new();
    }
}
