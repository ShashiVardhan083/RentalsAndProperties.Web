using Microsoft.AspNetCore.Mvc;
using RentalsAndProperties.Web.Filters;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Services;
using RentalsAndProperties.Web.ViewModels.Chat;

namespace RentalsAndProperties.Web.Controllers
{
    [JwtAuthorize]
    public class ChatController : Controller
    {
        private readonly ChatApiService ChatApi;
        private readonly IHttpContextAccessor HttpContextAccessor;

        public ChatController(ChatApiService chatApi, IHttpContextAccessor httpContextAccessor)
        {
            ChatApi = chatApi;
            HttpContextAccessor = httpContextAccessor;
        }

        // Conversation List
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await ChatApi.GetConversationsAsync();

            var conversations = result?.Data ?? new List<ChatConversationSummaryDto>();

            var vm = conversations.Select(c => new ConversationListViewModel
            {
                OtherUserId = c.OtherUserId,
                OtherUserName = c.OtherUserName,
                PropertyId = c.PropertyId,
                PropertyTitle = c.PropertyTitle,
                LastMessage = c.LastMessage,
                LastMessageAt = c.LastMessageAt,
                UnreadCount = c.UnreadCount
            }).ToList();

            return View(vm);
        }

        // Chat Conversation
        [HttpGet]
        public async Task<IActionResult> Conversation(
    Guid propertyId,
    Guid otherUserId,
    string otherUserName = "",
    string propertyTitle = "")
        {
            var userIdStr = HttpContextAccessor.HttpContext!.Session.GetString("UserId");
            Guid.TryParse(userIdStr, out var currentUserId);

            // Prevent self chat
            if (currentUserId == otherUserId)
            {
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }

            var result = await ChatApi.GetMessagesAsync(propertyId, otherUserId);
            var messageDtos = result?.Data ?? new List<ChatMessageDto>();

            var messages = messageDtos.Select(m => new ChatMessageViewModel
            {
                MessageId = m.MessageId,
                SenderId = m.SenderId,
                SenderName = m.SenderName,
                ReceiverId = m.ReceiverId,
                PropertyId = m.PropertyId,
                MessageText = m.MessageText,
                CreatedAt = m.CreatedAt,
                IsRead = m.IsRead,
                IsMine = m.SenderId == currentUserId
            }).ToList();

            var vm = new ChatConversationViewModel
            {
                OtherUserId = otherUserId,
                OtherUserName = otherUserName,
                PropertyId = propertyId,
                PropertyTitle = propertyTitle,
                Messages = messages,
                CurrentUserId = currentUserId
            };

            return View(vm);
        }
    }
}