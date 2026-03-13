namespace RentalsAndProperties.Web.Models.Dtos
{
    public class PropertyImageDto

    {
        public Guid ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
