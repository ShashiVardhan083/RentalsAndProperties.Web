using RentalsAndProperties.Web.ViewModels.Property;

namespace RentalsAndProperties.Web.Models.Dtos
{
    public class PropertySearchResultDto
    {
        public List<PropertyCardViewModel> Properties { get; set; } = new();

        public int TotalCount { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public bool HasPrevious { get; set; }

        public bool HasNext { get; set; }

        public PropertySearchQueryDto AppliedFilters { get; set; } = new();   
    }
}