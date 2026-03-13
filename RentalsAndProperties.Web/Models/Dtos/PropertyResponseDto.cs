namespace RentalsAndProperties.Web.Models.Dtos
{
    public class PropertyResponseDto
    {
        public Guid PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string ListingType { get; set; } = string.Empty;
        public string BHKType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PrimaryImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ImageCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        //  Add this — must match the API's PropertyResponseDto
        public List<string> ImageUrls { get; set; } = new();
    }
}



/*namespace RentalsAndProperties.Web.Models.Dtos
{
    public class PropertyResponseDto
    {
        public Guid PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public string ListingType { get; set; } = string.Empty;
        public string BHKType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PrimaryImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ImageCount { get; set; }

        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
*/
