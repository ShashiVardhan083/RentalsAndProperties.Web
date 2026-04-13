using System.Text.Json;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;

namespace RentalsAndProperties.Web.Services
{
    public class PropertySearchApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<PropertySearchApiService> Logger;
        private readonly string ApiBase;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PropertySearchApiService(HttpClient http, ILogger<PropertySearchApiService> logger, IConfiguration config)
        {
            HttpClient = http;
            Logger = logger;
            ApiBase = config["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "";
        }

        private string ResolveImageUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            if (url.StartsWith("http://") || url.StartsWith("https://")) return url;
            return ApiBase + "/" + url.TrimStart('/');
        }

        public async Task<ApiResponseModel<PropertySearchResultDto>?> SearchAsync(
            PropertySearchQueryDto propertySearchQueryDto)
        {
            var queryString = BuildQueryString(propertySearchQueryDto);
            var response = await HttpClient.GetAsync($"api/property/search?{queryString}");
            var result = await ReadAsync<PropertySearchResultDto>(response);

            if (result?.Data?.Properties != null)
            {
                foreach (var prop in result.Data.Properties)
                {
                    prop.PrimaryImageUrl = ResolveImageUrl(prop.PrimaryImageUrl);
                    prop.ImageUrls = prop.ImageUrls
                        .Select(url => ResolveImageUrl(url))
                        .ToList();
                }
            }

            return result;
        }

        private static string BuildQueryString(PropertySearchQueryDto propertySearchQueryDto)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(propertySearchQueryDto.City))
                parts.Add($"city={Uri.EscapeDataString(propertySearchQueryDto.City)}");
            if (propertySearchQueryDto.MinPrice.HasValue) parts.Add($"minPrice={propertySearchQueryDto.MinPrice}");
            if (propertySearchQueryDto.MaxPrice.HasValue) parts.Add($"maxPrice={propertySearchQueryDto.MaxPrice}");
            if (!string.IsNullOrWhiteSpace(propertySearchQueryDto.BHKType)) parts.Add($"bhkType={propertySearchQueryDto.BHKType}");
            if (!string.IsNullOrWhiteSpace(propertySearchQueryDto.PropertyType)) parts.Add($"propertyType={propertySearchQueryDto.PropertyType}");
            if (!string.IsNullOrWhiteSpace(propertySearchQueryDto.ListingType)) parts.Add($"listingType={propertySearchQueryDto.ListingType}");
            if (!string.IsNullOrWhiteSpace(propertySearchQueryDto.FurnishingType)) parts.Add($"furnishingType={propertySearchQueryDto.FurnishingType}");
            if (propertySearchQueryDto.MinBedrooms.HasValue) parts.Add($"minBedrooms={propertySearchQueryDto.MinBedrooms}");
            if (propertySearchQueryDto.MinBathrooms.HasValue) parts.Add($"minBathrooms={propertySearchQueryDto.MinBathrooms}");
            if (!string.IsNullOrWhiteSpace(propertySearchQueryDto.SortBy)) parts.Add($"sortBy={propertySearchQueryDto.SortBy}");
            parts.Add($"page={propertySearchQueryDto.Page}");
            parts.Add($"pageSize={propertySearchQueryDto.PageSize}");
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