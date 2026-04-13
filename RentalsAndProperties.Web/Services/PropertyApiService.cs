using System.Text;
using System.Text.Json;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;
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

        public async Task<ApiResponseModel<PropertyDetailDto>?> ApproveRejectPropertyAsync(Guid propertyId, string? reason, Boolean isApproved)
        {
            var payload = new { Reason = reason, IsApproved = isApproved };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync($"api/admin/property/{propertyId}/approve-reject",content);
            return await ReadAsync<PropertyDetailDto>(response);
        }

        // Multipart form builders

        private static MultipartFormDataContent BuildMultipartForm(CreatePropertyViewModel createPropertyViewModel)
        {
            var form = new MultipartFormDataContent();

            form.Add(new StringContent(createPropertyViewModel.Title ?? ""), "Title");
            form.Add(new StringContent(createPropertyViewModel.Description ?? ""), "Description");
            form.Add(new StringContent((createPropertyViewModel.City ?? 0).ToString()), "City");
            form.Add(new StringContent(createPropertyViewModel.Address ?? ""), "Address");
            form.Add(new StringContent((createPropertyViewModel.Price ?? 0).ToString()), "Price");
            form.Add(new StringContent((createPropertyViewModel.SecurityDeposit ?? 0).ToString()), "SecurityDeposit");
            form.Add(new StringContent(createPropertyViewModel.SquareFeet.ToString()), "SquareFeet");
            form.Add(new StringContent(createPropertyViewModel.Bedrooms.ToString()), "Bedrooms");
            form.Add(new StringContent(createPropertyViewModel.Bathrooms.ToString()), "Bathrooms");
            form.Add(new StringContent(((int)(createPropertyViewModel.PropertyType ?? 0)).ToString()), "PropertyType");
            form.Add(new StringContent(((int)(createPropertyViewModel.ListingType ?? 0)).ToString()), "ListingType");
            form.Add(new StringContent(((int)(createPropertyViewModel.BHKType ?? 0)).ToString()), "BHKType");
            form.Add(new StringContent(((int)(createPropertyViewModel.FurnishingType ?? 0)).ToString()), "FurnishingType");
            form.Add(new StringContent(createPropertyViewModel.AvailableFrom.ToString("O")), "AvailableFrom");

            if (createPropertyViewModel.Images != null)
            {
                foreach (var img in createPropertyViewModel.Images.Where(i => i.Length > 0))
                {
                    var stream = img.OpenReadStream();//Reads the file from memory/request->Converts it into a stream(binary data)
                    var sc = new StreamContent(stream);//Converts the stream into something HTTP can send
                    sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(img.ContentType);//Server needs to know what type of file this is
                    form.Add(sc, "Images", img.FileName);
                }
            }

            return form;
        }

        private static MultipartFormDataContent BuildMultipartFormForEdit(EditPropertyViewModel editPropertyViewModel)
        {
            var form = new MultipartFormDataContent();

            form.Add(new StringContent(editPropertyViewModel.Title ?? ""), "Title");
            form.Add(new StringContent(editPropertyViewModel.Description ?? ""), "Description");
            form.Add(new StringContent(editPropertyViewModel.City.HasValue ? ((int)editPropertyViewModel.City.Value).ToString() : "" ), "City");
            form.Add(new StringContent(editPropertyViewModel.Pincode ?? ""), "Pincode");
            form.Add(new StringContent(editPropertyViewModel.Address ?? ""), "Address");
            form.Add(new StringContent((editPropertyViewModel.Price ?? 0).ToString()), "Price");
            form.Add(new StringContent((editPropertyViewModel.SecurityDeposit ?? 0).ToString()), "SecurityDeposit");
            form.Add(new StringContent(editPropertyViewModel.SquareFeet.ToString()), "SquareFeet");
            form.Add(new StringContent(editPropertyViewModel.Bedrooms.ToString()), "Bedrooms");
            form.Add(new StringContent(editPropertyViewModel.Bathrooms.ToString()), "Bathrooms");
            form.Add(new StringContent(((int)(editPropertyViewModel.PropertyType ?? 0)).ToString()), "PropertyType");
            form.Add(new StringContent(((int)(editPropertyViewModel.ListingType ?? 0)).ToString()), "ListingType");
            form.Add(new StringContent(((int)(editPropertyViewModel.BHKType ?? 0 )).ToString()), "BHKType");
            form.Add(new StringContent(((int)(editPropertyViewModel.FurnishingType ?? 0)).ToString()), "FurnishingType");
            form.Add(new StringContent(editPropertyViewModel.AvailableFrom.ToString("O")), "AvailableFrom");

            if (editPropertyViewModel.Images != null)
            {
                foreach (var img in editPropertyViewModel.Images.Where(i => i?.Length > 0))
                {
                    var stream = img.OpenReadStream(); //Reads the file from memory/request->Converts it into a stream(binary data)
                    var sc = new StreamContent(stream); //Converts the stream into something HTTP can send
                    sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(img.ContentType); //Server needs to know what type of file this is
                    form.Add(sc, "Images", img.FileName);
                }
            }

            return form;
        }

        // Response reader

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