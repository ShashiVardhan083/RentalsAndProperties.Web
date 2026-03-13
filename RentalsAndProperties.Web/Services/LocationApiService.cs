using System.Text.Json;

namespace RentalsAndProperties.Web.Services
{
    public class LocationApiService
    {
        private readonly HttpClient HttpClient;

        public LocationApiService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<string?> GetPincodeAsync(string city)
        {
            var response = await HttpClient.GetAsync($"api/location/pincode?city={city}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<PincodeResult>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return data?.Pincode;
        }
    }

    public class PincodeResult
    {
        public string? Pincode { get; set; }
    }
}