using System.Text;
using System.Text.Json;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;

namespace RentalsAndProperties.Web.Services
{
    public class ChatApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<ChatApiService> Logger;

        private static readonly JsonSerializerOptions Opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ChatApiService(HttpClient http, ILogger<ChatApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        // Get conversation list
        public async Task<ApiResponseModel<List<ChatConversationSummaryDto>>?> GetConversationsAsync()
        {
            var response = await HttpClient.GetAsync("api/chat/conversations");
            return await ReadAsync<List<ChatConversationSummaryDto>>(response);
        }

        // Get messages for a conversation
        public async Task<ApiResponseModel<List<ChatMessageDto>>?> GetMessagesAsync(
            Guid propertyId,
            Guid otherUserId)
        {
            var response = await HttpClient.GetAsync($"api/chat/messages/{propertyId}/{otherUserId}");
            return await ReadAsync<List<ChatMessageDto>>(response);
        }

        // Send message
        public async Task<ApiResponseModel<ChatMessageDto>?> SendMessageAsync(
            Guid receiverId,
            Guid propertyId,
            string messageText)
        {
            var payload = new
            {
                ReceiverId = receiverId,
                PropertyId = propertyId,
                MessageText = messageText
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await HttpClient.PostAsync("api/chat/send", content);

            return await ReadAsync<ChatMessageDto>(response);
        }

        // Generic reader
        private async Task<ApiResponseModel<T>?> ReadAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(body, Opts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Chat API error");

                return new ApiResponseModel<T>
                {
                    Success = false,
                    Message = "Unable to reach chat service."
                };
            }
        }
    }
}