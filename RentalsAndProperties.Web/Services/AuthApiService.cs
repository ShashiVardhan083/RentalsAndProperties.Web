using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;
using static System.Net.WebRequestMethods;

namespace RentalsAndProperties.Web.Services
{
    public class AuthApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<AuthApiService> Logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthApiService(HttpClient http, ILogger<AuthApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        //POST /api/auth/register – send OTP to phone
        public async Task<ApiResponseModel<OtpSendResultDto>?> SendOtpAsync(string phoneNumber)
        {
            var payload = new { phoneNumber };
            return await PostAsync<OtpSendResultDto>("api/auth/register", payload);
        }

        //POST /api/auth/verify-otp – verify OTP + create account
        public async Task<ApiResponseModel<AuthResponseDto>?> VerifyOtpAsync(
            string phoneNumber,
            string otpCode,
            string fullName,
            string? email,
            string password,
            string confirmPassword)
        {
            var payload = new
            {
                phoneNumber,
                otpCode,
                fullName,
                email,
                password,
                confirmPassword
            };
            return await PostAsync<AuthResponseDto>("api/auth/verify-otp", payload);
        }

        //POST /api/auth/login – password login
        public async Task<ApiResponseModel<AuthResponseDto>?> LoginAsync(
            string phoneNumber,
            string password)
        {
            var payload = new { phoneNumber, password };
            return await PostAsync<AuthResponseDto>("api/auth/login", payload);
        }

        //POST /api/auth/logout
        public async Task<bool> LogoutAsync(string token)
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await HttpClient.PostAsync("api/auth/logout", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<ApiResponseModel<AuthResponseDto>?> BecomeOwnerAsync()
        {
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, "application/json");

                var response = await HttpClient.PostAsync("api/auth/become-owner", content);

                var body = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ApiResponseModel<AuthResponseDto>>(body, JsonOpts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "BecomeOwner API call failed.");
                return null;
            }
        }
        //  Generic helpers

        private async Task<ApiResponseModel<T>?> PostAsync<T>(string url, object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json"); 
                var response = await HttpClient.PostAsync(url, content);
                var body = await response.Content.ReadAsStringAsync();

                Logger.LogDebug("POST {Url} - {Status}", url, response.StatusCode);

                return JsonSerializer.Deserialize<ApiResponseModel<T>>(body, JsonOpts);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "API call to {Url} failed", url);
                return new ApiResponseModel<T>
                {
                    Success = false,
                    Message = "Unable to reach the server. Please try again.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
