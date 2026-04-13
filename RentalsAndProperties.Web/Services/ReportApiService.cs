using System.Text;
using System.Text.Json;
using RentalsAndProperties.Web.Models;
using RentalsAndProperties.Web.Models.Dtos;

namespace RentalsAndProperties.Web.Services
{
    public class ReportApiService
    {
        private readonly HttpClient HttpClient;
        private readonly ILogger<ReportApiService> Logger;

        private static readonly JsonSerializerOptions Opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReportApiService(HttpClient http, ILogger<ReportApiService> logger)
        {
            HttpClient = http;
            Logger = logger;
        }

        public async Task<ApiResponseModel<ReportResponseDto>?> CreateReportAsync(
            string targetType,
            Guid targetId,
            string? targetTitle,
            string reason,
            string? description)
        {
            var payload = new
            {
                TargetType = targetType,
                TargetId = targetId,
                TargetTitle= targetTitle ?? string.Empty,
                Reason = reason,
                Description = description
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await HttpClient.PostAsync("api/reports", content);

            return await ReadAsync<ReportResponseDto>(response);
        }

        public async Task<ApiResponseModel<List<ReportResponseDto>>?> GetMyReportsAsync()
        {
            var response = await HttpClient.GetAsync("api/reports/myReports");

            return await ReadAsync<List<ReportResponseDto>>(response);
        }

        public async Task<ApiResponseModel<List<ReportResponseDto>>?> GetAllReportsAsync(string? filter = null)
        {
            var url = string.IsNullOrEmpty(filter)
                ? "api/admin/reports"
                : $"api/admin/reports?filter={filter}";

            var response = await HttpClient.GetAsync(url);

            return await ReadAsync<List<ReportResponseDto>>(response);
        }

        public async Task<ApiResponseModel<ReportResponseDto>?> ResolveReportAsync(Guid reportId)
        {
            var response = await HttpClient.PostAsync($"api/admin/reports/{reportId}/resolve", null);

            return await ReadAsync<ReportResponseDto>(response);
        }

        public async Task<ApiResponseModel<ReportResponseDto>?> RejectReportAsync(Guid reportId)
        {
            var response = await HttpClient.PostAsync($"api/admin/reports/{reportId}/reject", null);

            return await ReadAsync<ReportResponseDto>(response);
        }

        private async Task<ApiResponseModel<T>?> ReadAsync<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<ApiResponseModel<T>>(body, Opts);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RAW RESPONSE: " + body);
                Logger.LogError(ex, "Report API error");

                return new ApiResponseModel<T>
                {
                    Success = false,
                    Message = "Server error."
                };
            }
        }
    }
}