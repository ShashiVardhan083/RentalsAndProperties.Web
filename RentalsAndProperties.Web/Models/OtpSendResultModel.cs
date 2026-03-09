namespace RentalsAndProperties.Web.Models
{
    public class OtpSendResultModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? DevOtp { get; set; }
    }
}
