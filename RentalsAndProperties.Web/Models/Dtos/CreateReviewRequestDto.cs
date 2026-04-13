namespace RentalsAndProperties.Web.Models.Dtos
{
    public class CreateReviewRequestDto
    {
        public Guid PropertyId { get; set; }
        public string ReviewType { get; set; } = "";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public int OwnerResponsiveness { get; set; }
        public int PropertyAccuracy { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
