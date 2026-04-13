namespace RentalsAndProperties.Web.Models.Dtos
{
    public class ReviewResponseDto
    {
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewType { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
