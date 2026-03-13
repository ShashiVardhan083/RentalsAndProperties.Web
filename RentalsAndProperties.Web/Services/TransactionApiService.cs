using System.Text;
using System.Text.Json;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.ViewModels.Transaction;

namespace RentalsAndProperties.Web.Services
{
    public class TransactionApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<TransactionApiService> Logger;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public TransactionApiService(HttpClient http, ILogger<TransactionApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        public async Task<ApiResponseModel<TransactionResponseDto>?> CreateAsync(
            TransactionViewModel vm)
        {
            var payload = new
            {
                PropertyId = vm.PropertyId,
                TransactionType = vm.TransactionType,
                PaymentMethod = vm.PaymentMethod
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync("api/transactions", content);
            return await ReadAsync<TransactionResponseDto>(response);
        }

        public async Task<ApiResponseModel<TransactionResponseDto>?> ConfirmAsync(Guid transactionId)
        {
            var response = await HttpClient.PostAsync($"api/transactions/{transactionId}/confirm", null);
            return await ReadAsync<TransactionResponseDto>(response);
        }

        public async Task<ApiResponseModel<List<TransactionResponseDto>>?> GetMyTransactionsAsync()
        {
            var response = await HttpClient.GetAsync("api/transactions/user");
            return await ReadAsync<List<TransactionResponseDto>>(response);
        }

        public async Task<ApiResponseModel<TransactionResponseDto>?> GetByIdAsync(Guid transactionId)
        {
            var response = await HttpClient.GetAsync($"api/transactions/{transactionId}");
            return await ReadAsync<TransactionResponseDto>(response);
        }

        private async Task<ApiResponseModel<T>?> ReadAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(body, JsonOpts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Transaction API error");
                return new ApiResponseModel<T> { Success = false, Message = "Unable to reach server." };
            }
        }
    }
}
