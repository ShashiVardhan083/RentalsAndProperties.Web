using RentalsAndProperties.Web.Models.Dtos;

namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class EditPropertyViewModel : CreatePropertyViewModel
    {
        public Guid PropertyId { get; set; }
        public List<PropertyImageViewModel> ExistingImages { get; set; } = new();
        public string CurrentStatus { get; set; } = string.Empty;
    }
}
