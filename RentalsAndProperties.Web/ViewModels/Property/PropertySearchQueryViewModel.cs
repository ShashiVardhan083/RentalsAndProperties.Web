namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class PropertySearchQueryViewModel
    {
        public string? City { get; set; }

        public string? ListingType { get; set; }

        public string? PropertyType { get; set; }

        public string? BHKType { get; set; }


        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; }

        public int Page { get; set; } = 1;
    }
}
