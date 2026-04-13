namespace RentalsAndProperties.Web.ViewModels.Report
{
    public class ReportViewModel
    {
        public Guid ReportId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public Guid TargetId { get; set; }
        public string TargetTitle { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}