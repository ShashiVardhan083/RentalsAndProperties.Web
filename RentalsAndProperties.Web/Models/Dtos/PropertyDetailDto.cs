namespace RentalsAndProperties.Web.Models.Dtos
{
    public class PropertyDetailDto
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
        public string? Pincode { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string ListingType { get; set; } = string.Empty;
        public string BHKType { get; set; } = string.Empty;
        public string FurnishingType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AvailableFrom { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public List<PropertyImageDto> Images { get; set; } = new();
    }
}
