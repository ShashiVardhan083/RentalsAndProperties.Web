using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RentalsAndProperties.Web.Models;

namespace RentalsAndProperties.Web.Services
{
    public class PropertyMediaApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<PropertyMediaApiService> Logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PropertyMediaApiService(HttpClient http, ILogger<PropertyMediaApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        public async Task<ApiResponseModel<List<PropertyImageModel>>?> GetImagesAsync(Guid propertyId)
        {
            var response = await HttpClient.GetAsync($"api/property/{propertyId}/images");
            return await ReadAsync<List<PropertyImageModel>>(response);
        }

        public async Task<ApiResponseModel<List<PropertyImageModel>>?> UploadImagesAsync(
    Guid propertyId, List<IFormFile> images)
        {
            using var form = new MultipartFormDataContent();

            foreach (var img in images)
            {
                if (img.Length == 0) continue;

                var stream = img.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(img.ContentType);

                form.Add(content, "Images", img.FileName); // lowercase is safer
            }


            var response = await HttpClient.PostAsync($"api/property/{propertyId}/images", form);

            return await ReadAsync<List<PropertyImageModel>>(response);
        }

        public async Task<ApiResponseModel<object>?> DeleteImageAsync(Guid imageId)
        {
            var response = await HttpClient.DeleteAsync($"api/property/image/{imageId}");
            return await ReadAsync<object>(response);
        }

        public async Task<ApiResponseModel<PropertyImageModel>?> SetPrimaryAsync(Guid imageId)
        {
            var response = await HttpClient.PutAsync(
                $"api/property/image/{imageId}/set-primary", null);
            return await ReadAsync<PropertyImageModel>(response);
        }

        private async Task<ApiResponseModel<T>?> ReadAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                Logger.LogDebug("{Method} {Url} → {Status}",
                    response.RequestMessage?.Method,
                    response.RequestMessage?.RequestUri,
                    response.StatusCode);
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