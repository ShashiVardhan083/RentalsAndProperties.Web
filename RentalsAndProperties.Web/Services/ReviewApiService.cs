using System.Text;
using System.Text.Json;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.ViewModels.Review;

namespace RentalsAndProperties.Web.Services
{
    public class ReviewApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<ReviewApiService> Logger;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public ReviewApiService(HttpClient http, ILogger<ReviewApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        public async Task<ApiResponseModel<ReviewViewModel>?> CreateAsync(ReviewViewModel vm)
        {
            var payload = new
            {
                PropertyId = vm.PropertyId,
                ReviewType = vm.ReviewType,
                Rating = vm.Rating,
                Comment = vm.Comment,
                OwnerResponsiveness = vm.OwnerResponsiveness,
                PropertyAccuracy = vm.PropertyAccuracy,
                TransactionId = vm.TransactionId,
                PriceSatisfaction = vm.PriceSatisfaction,
                WouldRecommend = vm.WouldRecommend
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync("api/reviews", content);
            return await ReadAsync<ReviewViewModel>(response);
        }

        public async Task<ApiResponseModel<List<ReviewViewModel>>?> GetPropertyReviewsAsync(Guid propertyId)
        {
            var response = await HttpClient.GetAsync($"api/reviews/property/{propertyId}");
            return await ReadAsync<List<ReviewViewModel>>(response);
        }

        public async Task<ApiResponseModel<List<ReviewViewModel>>?> GetMyReviewsAsync()
        {
            var response = await HttpClient.GetAsync("api/reviews/user");
            return await ReadAsync<List<ReviewViewModel>>(response);
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
                Logger.LogError(ex, "Review API error");
                return new ApiResponseModel<T> { Success = false, Message = "Unable to reach server." };
            }
        }
    }
}
