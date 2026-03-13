namespace RentalsAndProperties.Web.Models.Dtos
{
    public class OtpSendResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? DevOtp { get; set; }
    }
}
