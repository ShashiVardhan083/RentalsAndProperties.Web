using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Models.Enums;
using RentalsAndProperties.Web.ViewModels.Property;

namespace RentalsAndProperties.Web.Services
{
    public class PropertyApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<PropertyApiService> Logger;
        private readonly string ApiBase;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PropertyApiService(HttpClient http, ILogger<PropertyApiService> logger, IConfiguration config)
        {
            HttpClient = http;
            Logger = logger;
            ApiBase = config["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "";
        }

        //  Helper: converts /uploads/...  https://localhost:APIPORT/uploads/...
        private string ResolveImageUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            if (url.StartsWith("http://") || url.StartsWith("https://")) return url;
            return ApiBase + "/" + url.TrimStart('/');
        }

        // Owner endpoints

        public async Task<ApiResponseModel<PropertyDetailDto>?> CreatePropertyAsync(
            CreatePropertyViewModel model)
        {
            using var form = BuildMultipartForm(model);
            var response = await HttpClient.PostAsync("api/property/AddProperty", form);
            var result = await ReadAsync<PropertyDetailDto>(response);

            if (result?.Data?.Images != null)
                foreach (var img in result.Data.Images)
                    img.ImageUrl = ResolveImageUrl(img.ImageUrl);

            return result;
        }

        public async Task<ApiResponseModel<PropertyDetailDto>?> UpdatePropertyAsync(
            Guid propertyId, EditPropertyViewModel model)
        {
            using var form = BuildMultipartFormForEdit(model);
            var response = await HttpClient.PutAsync($"api/property/UpdateProperty/{propertyId}", form);
            var result = await ReadAsync<PropertyDetailDto>(response);

            if (result?.Data?.Images != null)
                foreach (var img in result.Data.Images)
                    img.ImageUrl = ResolveImageUrl(img.ImageUrl);

            return result;
        }

        public async Task<ApiResponseModel<object>?> DeletePropertyAsync(Guid propertyId)
        {
            var response = await HttpClient.DeleteAsync($"api/property/DeleteProperty/{propertyId}");
            return await ReadAsync<object>(response);
        }

        public async Task<ApiResponseModel<List<PropertyResponseDto>>?> GetOwnerPropertiesAsync()
        {
            var response = await HttpClient.GetAsync("api/property/owner");
            var result = await ReadAsync<List<PropertyResponseDto>>(response);

            // ✅ Fix image URLs for dashboard thumbnails and image count
            if (result?.Data != null)
            {
                foreach (var prop in result.Data)
                {
                    prop.PrimaryImageUrl = ResolveImageUrl(prop.PrimaryImageUrl);
                    prop.ImageUrls = prop.ImageUrls
                        .Select(url => ResolveImageUrl(url))
                        .ToList();
                }
            }

            return result;
        }

        // Public/shared endpoints

        public async Task<ApiResponseModel<PagedResultDto<PropertyResponseDto>>?> GetApprovedAsync(
            int page = 1, int pageSize = 12)
        {
            var response = await HttpClient.GetAsync($"api/property/approved?page={page}&pageSize={pageSize}");
            var result = await ReadAsync<PagedResultDto<PropertyResponseDto>>(response);

            //image URLs for public listing cards
            if (result?.Data?.Items != null)
            {
                foreach (var prop in result.Data.Items)
                {
                    prop.PrimaryImageUrl = ResolveImageUrl(prop.PrimaryImageUrl);
                    prop.ImageUrls = prop.ImageUrls
                        .Select(url => ResolveImageUrl(url))
                        .ToList();
                }
            }

            return result;
        }

        public async Task<ApiResponseModel<PropertyDetailDto>?> GetDetailsAsync(Guid propertyId)
        {
            var response = await HttpClient.GetAsync($"api/property/{propertyId}");
            var body = await response.Content.ReadAsStringAsync();

            Logger.LogWarning("GetDetailsAsync RAW RESPONSE: {Body}", body);

            var result = JsonSerializer.Deserialize<ApiResponseModel<PropertyDetailDto>>(body, JsonOpts);

            Logger.LogWarning("GetDetailsAsync Images count: {Count}",
                result?.Data?.Images?.Count ?? -1);

            if (result?.Data?.Images != null)
                foreach (var img in result.Data.Images)
                {
                    Logger.LogWarning("Image before resolve: {Url}", img.ImageUrl);
                    img.ImageUrl = ResolveImageUrl(img.ImageUrl);
                    Logger.LogWarning("Image after resolve: {Url}", img.ImageUrl);
                }

            return result;
        }

        // Admin endpoints

        public async Task<ApiResponseModel<List<PropertyDetailDto>>?> GetPendingAsync()
        {
            var response = await HttpClient.GetAsync("api/admin/properties/pending");
            var result = await ReadAsync<List<PropertyDetailDto>>(response);

            if (result?.Data != null)
            {
                foreach (var prop in result.Data)
                    foreach (var img in prop.Images)
                        img.ImageUrl = ResolveImageUrl(img.ImageUrl);
            }

            return result;
        }

        public async Task<ApiResponseModel<PropertyDetailDto>?> ApprovePropertyAsync(Guid propertyId)
        {
            var response = await HttpClient.PostAsync($"api/admin/property/{propertyId}/approve", null);
            return await ReadAsync<PropertyDetailDto>(response);
        }

        public async Task<ApiResponseModel<PropertyDetailDto>?> RejectPropertyAsync(Guid propertyId, string reason)
        {
            var payload = new { Reason = reason };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync($"api/admin/property/{propertyId}/reject", content);
            return await ReadAsync<PropertyDetailDto>(response);
        }

        // Multipart form builders

        private static MultipartFormDataContent BuildMultipartForm(CreatePropertyViewModel m)
        {
            var form = new MultipartFormDataContent();

            form.Add(new StringContent(m.Title ?? ""), "Title");
            form.Add(new StringContent(m.Description ?? ""), "Description");
            form.Add(new StringContent(m.City.ToString()), "City");
            form.Add(new StringContent(m.Address ?? ""), "Address");
            form.Add(new StringContent(m.Price.ToString()), "Price");
            form.Add(new StringContent((m.SecurityDeposit ?? 0).ToString()), "SecurityDeposit");
            form.Add(new StringContent(m.SquareFeet.ToString()), "SquareFeet");
            form.Add(new StringContent(m.Bedrooms.ToString()), "Bedrooms");
            form.Add(new StringContent(m.Bathrooms.ToString()), "Bathrooms");
            form.Add(new StringContent(((int)m.PropertyType).ToString()), "PropertyType");
            form.Add(new StringContent(((int)m.ListingType).ToString()), "ListingType");
            form.Add(new StringContent(((int)m.BHKType).ToString()), "BHKType");
            form.Add(new StringContent(((int)m.FurnishingType).ToString()), "FurnishingType");
            form.Add(new StringContent(m.AvailableFrom.ToString("O")), "AvailableFrom");

            if (m.Images != null)
            {
                foreach (var img in m.Images.Where(i => i?.Length > 0))
                {
                    var stream = img.OpenReadStream();
                    var sc = new StreamContent(stream);
                    sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(img.ContentType);
                    form.Add(sc, "Images", img.FileName);
                }
            }

            return form;
        }

        private static MultipartFormDataContent BuildMultipartFormForEdit(EditPropertyViewModel m)
        {
            var form = new MultipartFormDataContent();

            form.Add(new StringContent(m.Title ?? ""), "Title");
            form.Add(new StringContent(m.Description ?? ""), "Description");
            form.Add(new StringContent(
                m.City.HasValue 
                ? CityDetailsEnum.GetName(m.City.Value) 
                : ""
                ), "City");
            form.Add(new StringContent(m.Pincode ?? ""), "Pincode");
            form.Add(new StringContent(m.Address ?? ""), "Address");
            form.Add(new StringContent(m.Price.ToString()), "Price");
            form.Add(new StringContent((m.SecurityDeposit ?? 0).ToString()), "SecurityDeposit");
            form.Add(new StringContent(m.SquareFeet.ToString()), "SquareFeet");
            form.Add(new StringContent(m.Bedrooms.ToString()), "Bedrooms");
            form.Add(new StringContent(m.Bathrooms.ToString()), "Bathrooms");
            form.Add(new StringContent(((int)m.PropertyType).ToString()), "PropertyType");
            form.Add(new StringContent(((int)m.ListingType).ToString()), "ListingType");
            form.Add(new StringContent(((int)m.BHKType).ToString()), "BHKType");
            form.Add(new StringContent(((int)m.FurnishingType).ToString()), "FurnishingType");
            form.Add(new StringContent(m.AvailableFrom.ToString("O")), "AvailableFrom");

            if (m.Images != null)
            {
                foreach (var img in m.Images.Where(i => i?.Length > 0))
                {
                    var stream = img.OpenReadStream();
                    var sc = new StreamContent(stream);
                    sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(img.ContentType);
                    form.Add(sc, "Images", img.FileName);
                }
            }

            return form;
        }

        //  Response reader

        private async Task<ApiResponseModel<T>?> ReadAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(body, JsonOpts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to read Property API response");
                return new ApiResponseModel<T>
                {
                    Success = false,
                    Message = "Unable to reach the server. Please try again."
                };
            }
        }
    }
}