public class ChatMessageViewModel
{
    public Guid MessageId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderName { get; set; } = "";

    public Guid ReceiverId { get; set; }

    public Guid PropertyId { get; set; }

    public string MessageText { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public bool IsMine { get; set; }
}