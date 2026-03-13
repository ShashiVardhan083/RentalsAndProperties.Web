namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class PropertyImageViewModel
    {
        public Guid ImageId { get; set; }

        public string ImageUrl { get; set; } = "";

        public bool IsPrimary { get; set; }
    }
}