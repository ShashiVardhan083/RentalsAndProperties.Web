namespace RentalsAndProperties.Web.Models
{
    public class PropertyImageModel
    {
        public Guid ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
