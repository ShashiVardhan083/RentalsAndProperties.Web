using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RentalsAndProperties.Web.Models;
using Microsoft.AspNetCore.Http;
using RentalsAndProperties.Web.Models.ViewModels;

namespace RentalsAndProperties.Web.Services
{
    public class PropertyApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<PropertyApiService> Logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PropertyApiService(HttpClient http, ILogger<PropertyApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        // Public

        public async Task<ApiResponseModel<PagedResultModel<PropertyResponseModel>>?> GetApprovedAsync(
            int page = 1, int pageSize = 12)
        {
            var response = await HttpClient.GetAsync($"api/property/approved?page={page}&pageSize={pageSize}");
            return await ReadAsync<PagedResultModel<PropertyResponseModel>>(response);
        }

        public async Task<ApiResponseModel<PropertyDetailModel>?> GetDetailsAsync(Guid id)
        {
            var response = await HttpClient.GetAsync($"api/property/{id}");
            return await ReadAsync<PropertyDetailModel>(response);
        }

        // Owner

        public async Task<ApiResponseModel<List<PropertyResponseModel>>?> GetOwnerPropertiesAsync()
        {
            var response = await HttpClient.GetAsync("api/property/owner");
            return await ReadAsync<List<PropertyResponseModel>>(response);
        }

        public async Task<ApiResponseModel<PropertyDetailModel>?> CreatePropertyAsync(
            CreatePropertyViewModel model)
        {
            using var form = BuildMultipartForm(model);
            AppendImages(form, model.Images);
            var response = await HttpClient.PostAsync("api/property/AddProperty", form);
            return await ReadAsync<PropertyDetailModel>(response);
        }

        public async Task<ApiResponseModel<PropertyDetailModel>?> UpdatePropertyAsync(
            Guid id, EditPropertyViewModel model)
        {
            using var form = BuildMultipartFormForEdit(model);
            AppendImages(form, model.Images);
            var response = await HttpClient.PutAsync($"api/property/UpdateProperty/{id}", form);
            return await ReadAsync<PropertyDetailModel>(response);
        }

        public async Task<ApiResponseModel<object>?> DeletePropertyAsync(Guid id)
        {
            var response = await HttpClient.DeleteAsync($"api/property/DeleteProperty/{id}");
            return await ReadAsync<object>(response);
        }

        // Admin

        public async Task<ApiResponseModel<List<PropertyDetailModel>>?> GetPendingAsync()
        {
            var response = await HttpClient.GetAsync("api/admin/properties/pending");
            return await ReadAsync<List<PropertyDetailModel>>(response);
        }

        public async Task<ApiResponseModel<PropertyDetailModel>?> ApprovePropertyAsync(Guid id)
        {
            var response = await HttpClient.PostAsync($"api/admin/property/{id}/approve", null);
            return await ReadAsync<PropertyDetailModel>(response);
        }

        public async Task<ApiResponseModel<PropertyDetailModel>?> RejectPropertyAsync(
            Guid id, string reason)
        {
            var payload = new { reason };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync($"api/admin/property/{id}/reject", content);
            return await ReadAsync<PropertyDetailModel>(response);
        }

        // Helpers

        private static MultipartFormDataContent BuildMultipartForm(CreatePropertyViewModel m)
        {
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(m.Title), "Title");
            form.Add(new StringContent(m.Description), "Description");
            form.Add(new StringContent(m.City), "City");
            form.Add(new StringContent(m.Address), "Address");
            form.Add(new StringContent(m.Price.ToString()), "Price");
            form.Add(new StringContent(m.SecurityDeposit?.ToString() ?? ""), "SecurityDeposit");
            form.Add(new StringContent(m.SquareFeet.ToString()), "SquareFeet");
            form.Add(new StringContent(m.Bedrooms.ToString()), "Bedrooms");
            form.Add(new StringContent(m.Bathrooms.ToString()), "Bathrooms");
            form.Add(new StringContent(((int)m.PropertyType).ToString()), "PropertyType");
            form.Add(new StringContent(((int)m.ListingType).ToString()), "ListingType");
            form.Add(new StringContent(((int)m.BHKType).ToString()), "BHKType");
            form.Add(new StringContent(((int)m.FurnishingType).ToString()), "FurnishingType");
            form.Add(new StringContent(m.AvailableFrom.ToString("o")), "AvailableFrom");
            return form;
        }

        private static MultipartFormDataContent BuildMultipartFormForEdit(EditPropertyViewModel m)
        {
            var form = BuildMultipartForm(m);
            return form;
        }

        private static void AppendImages(MultipartFormDataContent form, List<IFormFile>? images)
        {
            if (images == null || images.Count == 0) return;
            foreach (var img in images)
            {
                if (img.Length == 0) continue;
                var stream = img.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(img.ContentType);
                form.Add(content, "Images", img.FileName);
            }
        }

        private async Task<ApiResponseModel<T>?> ReadAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                Logger.LogDebug("{Method} {Url} - {Status}",
                    response.RequestMessage?.Method,
                    response.RequestMessage?.RequestUri,
                    response.StatusCode);
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(body, JsonOpts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to read API response");
                return new ApiResponseModel<T>
                {
                    Success = false,
                    Message = "Unable to reach the server. Please try again."
                };
            }
        }
    }
}