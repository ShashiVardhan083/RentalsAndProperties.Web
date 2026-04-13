namespace RentalsAndProperties.Web.Models.Dtos
{
    public class PropertySearchResultDto
    {
        public List<PropertyCardDto> Properties { get; set; } = new();

        public int TotalCount { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }  
    }
}