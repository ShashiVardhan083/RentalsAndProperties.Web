using System.Text;
using System.Text.Json;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;

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

        public async Task<ApiResponseModel<ReviewResponseDto>?> CreateAsync(CreateReviewRequestDto createReviewRequestDto)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(createReviewRequestDto),
                Encoding.UTF8,
                "application/json"
            );
            var response = await HttpClient.PostAsync("api/reviews", content);
            return await ReadAsync<ReviewResponseDto>(response);
        }

        public async Task<ApiResponseModel<List<ReviewResponseDto>>?> GetPropertyReviewsAsync(Guid propertyId)
        {
            var response = await HttpClient.GetAsync($"api/reviews/property/{propertyId}");
            return await ReadAsync<List<ReviewResponseDto>>(response);
        }

        public async Task<ApiResponseModel<List<ReviewResponseDto>>?> GetMyReviewsAsync()
        {
            var response = await HttpClient.GetAsync("api/reviews/user");
            return await ReadAsync<List<ReviewResponseDto>>(response);
        }

        private async Task<ApiResponseModel<T>?> ReadAsync<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            try
            {
                // Only try to deserialize if it's a success response
                if (response.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<ApiResponseModel<T>>(body, JsonOpts);

                // For error responses, return a clean message
                Logger.LogWarning("API {Status}: {Body}", response.StatusCode, body);
                return new ApiResponseModel<T>
                {
                    Success = false,
                    Message = $"Request failed ({response.StatusCode})."
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Deserialize error. Body: {Body}", body);
                return new ApiResponseModel<T> { Success = false, Message = "Unable to reach server." };
            }
        }
    }
}
