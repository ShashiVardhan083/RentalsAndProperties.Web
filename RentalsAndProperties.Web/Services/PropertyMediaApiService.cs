using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;

namespace RentalsAndProperties.Web.Services
{
    public class PropertyMediaApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<PropertyMediaApiService> Logger;
        private readonly string ApiBase;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PropertyMediaApiService(HttpClient http, ILogger<PropertyMediaApiService> logger, IConfiguration config)
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

        public async Task<ApiResponseModel<List<PropertyImageDto>>?> GetImagesAsync(Guid propertyId)
        {
            var response = await HttpClient.GetAsync($"api/property/{propertyId}/images");
            var result = await ReadAsync<List<PropertyImageDto>>(response);

            // ✅ Fix image URLs for UploadImages manage page
            if (result?.Data != null)
                foreach (var img in result.Data)
                    img.ImageUrl = ResolveImageUrl(img.ImageUrl);

            return result;
        }

        public async Task<ApiResponseModel<List<PropertyImageDto>>?> UploadImagesAsync(
            Guid propertyId, List<IFormFile> images)
        {
            using var form = new MultipartFormDataContent();
            foreach (var img in images)
            {
                if (img.Length == 0) continue;
                var stream = img.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(img.ContentType) ? "application/octet-stream" : img.ContentType);
                form.Add(content, "files", img.FileName);
            }

            var response = await HttpClient.PostAsync($"api/property/{propertyId}/images", form);
            var result = await ReadAsync<List<PropertyImageDto>>(response);

            // ✅ Fix image URLs on freshly uploaded images
            if (result?.Data != null)
                foreach (var img in result.Data)
                    img.ImageUrl = ResolveImageUrl(img.ImageUrl);

            return result;
        }

        public async Task<ApiResponseModel<object>?> DeleteImageAsync(Guid imageId)
        {
            var response = await HttpClient.DeleteAsync($"api/property/image/{imageId}");
            return await ReadAsync<object>(response);
        }

        public async Task<ApiResponseModel<PropertyImageDto>?> SetPrimaryAsync(Guid imageId)
        {
            var response = await HttpClient.PutAsync(
                $"api/property/image/{imageId}/set-primary", null);
            var result = await ReadAsync<PropertyImageDto>(response);

            // ✅ Fix image URL on primary image response
            if (result?.Data != null)
                result.Data.ImageUrl = ResolveImageUrl(result.Data.ImageUrl);

            return result;
        }

        private async Task<ApiResponseModel<T>?> ReadAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                Logger.LogDebug("{Method} {Url} → {Status} | Body: {Body}",
                    response.RequestMessage?.Method,
                    response.RequestMessage?.RequestUri,
                    response.StatusCode,
                    body);
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(body, JsonOpts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to read PropertyMedia API response");
                return new ApiResponseModel<T>
                {
                    Success = false,
                    Message = "Unable to reach the server. Please try again."
                };
            }
        }
    }
}