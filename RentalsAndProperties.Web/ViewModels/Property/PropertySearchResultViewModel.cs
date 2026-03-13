namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class PropertySearchResultViewModel
    {
        public List<PropertyCardViewModel> Properties { get; set; } = new();

        public int TotalCount { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }
}
