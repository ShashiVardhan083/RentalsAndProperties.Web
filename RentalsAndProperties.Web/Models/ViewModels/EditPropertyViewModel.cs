namespace RentalsAndProperties.Web.Models.ViewModels
{
    public class EditPropertyViewModel : CreatePropertyViewModel
    {
        public Guid PropertyId { get; set; }
        public List<PropertyImageModel> ExistingImages { get; set; } = new();
        public string CurrentStatus { get; set; } = string.Empty;
    }
}
