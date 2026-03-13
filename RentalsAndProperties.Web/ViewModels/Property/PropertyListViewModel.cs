namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class PropertyListViewModel
    {
        public List<PropertyCardViewModel> Properties { get; set; } = new();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }

        public bool HasPrevious => CurrentPage > 1;

        public bool HasNext => CurrentPage < TotalPages;
    }
}