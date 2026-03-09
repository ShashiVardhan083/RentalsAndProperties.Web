namespace RentalsAndProperties.Web.Models
{
    public class PropertySearchQueryModel
    {
        public string? City { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? BHKType { get; set; }
        public string? PropertyType { get; set; }
        public string? ListingType { get; set; }
        public string? FurnishingType { get; set; }
        public int? MinBedrooms { get; set; }
        public int? MinBathrooms { get; set; }
        public string SortBy { get; set; } = "Newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
