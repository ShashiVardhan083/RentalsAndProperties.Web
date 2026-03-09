namespace RentalsAndProperties.Web.Models
{
    public class PropertySearchResultModel
    {
        public List<PropertyCardModel> Properties { get; set; } = new();

        public int TotalCount { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public bool HasPrevious { get; set; }

        public bool HasNext { get; set; }

        public PropertySearchQueryModel AppliedFilters { get; set; } = new();
    }
}