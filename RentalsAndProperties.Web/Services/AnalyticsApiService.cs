using System.Text.Json;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;
namespace RentalsAndProperties.Web.Services
{
    public class AnalyticsApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<AnalyticsApiService> Logger;
        private static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };

        public AnalyticsApiService(HttpClient http, ILogger<AnalyticsApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        public async Task<ApiResponseModel<AdminAnalyticsDto>?> GetAnalyticsAsync()
        {
            try
            {
                var response = await HttpClient.GetAsync("api/admin/analytics");

                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError("Analytics API failed. Status: {status}", response.StatusCode);
                    return new ApiResponseModel<AdminAnalyticsDto>
                    {
                        Success = false,
                        Message = $"API Error: {response.StatusCode}"
                    };
                }

                var body = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponseModel<AdminAnalyticsDto>>(body, Opts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Analytics API call failed");

                return new ApiResponseModel<AdminAnalyticsDto>
                {
                    Success = false,
                    Message = "Analytics service unavailable."
                };
            }
        }
        public async Task<ApiResponseModel<List<UserDto>>?> GetAllUsersAsync()
        {
            try
            {
                var response = await HttpClient.GetAsync("api/admin/analytics/allUsers");

                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError("AllUsers API failed. Status: {status}", response.StatusCode);
                    return new ApiResponseModel<List<UserDto>>
                    {
                        Success = false,
                        Message = $"API Error: {response.StatusCode}"
                    };
                }

                var body = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponseModel<List<UserDto>>>(body, Opts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetAllUsers API call failed");

                return new ApiResponseModel<List<UserDto>>
                {
                    Success = false,
                    Message = "GetAllUsers service unavailable."
                };
            }
        }
    }
}