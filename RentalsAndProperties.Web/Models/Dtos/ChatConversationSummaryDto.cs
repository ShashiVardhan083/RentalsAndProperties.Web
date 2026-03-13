namespace RentalsAndProperties.Web.Models.Dtos
{
    public class ChatConversationSummaryDto
    {
        public Guid OtherUserId { get; set; }

        public string OtherUserName { get; set; } = string.Empty;

        public Guid PropertyId { get; set; }

        public string PropertyTitle { get; set; } = string.Empty;

        public string LastMessage { get; set; } = string.Empty;

        public DateTime LastMessageAt { get; set; }

        public int UnreadCount { get; set; }
    }
}
