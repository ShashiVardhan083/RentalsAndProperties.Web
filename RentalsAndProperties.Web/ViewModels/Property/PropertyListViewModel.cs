namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class PropertyListViewModel
    {
        public List<PropertyCardViewModel> Properties { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public bool IsAuthenticated { get; set; }
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();

        public bool IsOwnerOrAdmin => Roles.Contains("Owner") || Roles.Contains("Admin");
        public bool IsCustomerOnly => IsAuthenticated && !IsOwnerOrAdmin;
    }
}