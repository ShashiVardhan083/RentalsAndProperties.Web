namespace RentalsAndProperties.Web.Models.Dtos
{
    public class ChatMessageDto
    {
        public Guid MessageId { get; set; }

        public Guid SenderId { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public Guid ReceiverId { get; set; }

        public Guid PropertyId { get; set; }

        public string MessageText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }
    }

    
}