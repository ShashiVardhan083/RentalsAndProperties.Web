namespace RentalsAndProperties.Web.Models.Dtos
{
    public class ReviewResponseDto
    {
        public Guid ReviewId { get; set; }
        public Guid PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewType { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int OwnerResponsiveness { get; set; }
        public int PropertyAccuracy { get; set; }
        public Guid? TransactionId { get; set; }
        public int? PriceSatisfaction { get; set; }
        public bool? WouldRecommend { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
