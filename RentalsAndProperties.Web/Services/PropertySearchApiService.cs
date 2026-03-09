using System.Text.Json;
using RentalsAndProperties.Web.Models;

namespace RentalsAndProperties.Web.Services
{
    public class PropertySearchApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<PropertySearchApiService> Logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PropertySearchApiService(HttpClient http, ILogger<PropertySearchApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        public async Task<ApiResponseModel<PropertySearchResultModel>?> SearchAsync(
            PropertySearchQueryModel query)
        {
            var qs = BuildQueryString(query);
            var response = await HttpClient.GetAsync($"api/property/search?{qs}");
            return await ReadAsync<PropertySearchResultModel>(response);
        }

        public async Task<ApiResponseModel<List<string>>?> GetCitiesAsync()
        {
            var response = await HttpClient.GetAsync("api/property/cities");
            return await ReadAsync<List<string>>(response);
        }

        private static string BuildQueryString(PropertySearchQueryModel q)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(q.City))
                parts.Add($"city={Uri.EscapeDataString(q.City)}");
            if (q.MinPrice.HasValue) parts.Add($"minPrice={q.MinPrice}");
            if (q.MaxPrice.HasValue) parts.Add($"maxPrice={q.MaxPrice}");
            if (!string.IsNullOrWhiteSpace(q.BHKType)) parts.Add($"bhkType={q.BHKType}");
            if (!string.IsNullOrWhiteSpace(q.PropertyType)) parts.Add($"propertyType={q.PropertyType}");
            if (!string.IsNullOrWhiteSpace(q.ListingType)) parts.Add($"listingType={q.ListingType}");
            if (!string.IsNullOrWhiteSpace(q.FurnishingType)) parts.Add($"furnishingType={q.FurnishingType}");
            if (q.MinBedrooms.HasValue) parts.Add($"minBedrooms={q.MinBedrooms}");
            if (q.MinBathrooms.HasValue) parts.Add($"minBathrooms={q.MinBathrooms}");
            if (!string.IsNullOrWhiteSpace(q.SortBy)) parts.Add($"sortBy={q.SortBy}");
            parts.Add($"page={q.Page}");
            parts.Add($"pageSize={q.PageSize}");
            return string.Join("&", parts);
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
                Logger.LogError(ex, "PropertySearch API error");
                return new ApiResponseModel<T>
                {
                    Success = false,
                    Message = "Search service unavailable. Please try again."
                };
            }
        }
    }
}