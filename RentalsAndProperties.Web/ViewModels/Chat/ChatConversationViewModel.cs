namespace RentalsAndProperties.Web.ViewModels.Chat
{
    public class ChatConversationViewModel
    {
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public Guid PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public List<ChatMessageViewModel> Messages { get; set; } = new();
        public Guid CurrentUserId { get; set; }
    }
}
