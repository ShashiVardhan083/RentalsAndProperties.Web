using System.ComponentModel.DataAnnotations;

namespace RentalsAndProperties.Web.ViewModels.Report
{
    public class CreateReportViewModel
    {
        [Required]
        public string TargetType { get; set; } = string.Empty;

        [Required]
        public Guid TargetId { get; set; }

        [Required, MaxLength(200)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        // For display
        public string? TargetTitle { get; set; }
    }
}