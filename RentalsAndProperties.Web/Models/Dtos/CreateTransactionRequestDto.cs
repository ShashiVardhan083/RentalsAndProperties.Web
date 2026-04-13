namespace RentalsAndProperties.Web.Models.Dtos
{
    public class CreateTransactionRequestDto
    {
        public Guid PropertyId { get; set; }
        public string TransactionType { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
    }
}
