using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.ViewModels.Report;

namespace RentalsAndProperties.Web.Mappings
{
    public static class ReportMapper
    {
        public static List<ReportViewModel> ToViewModel(List<ReportResponseDto> reportResponseDtoList)
        {
            return reportResponseDtoList.Select(r => new ReportViewModel
            {
                ReportId = r.ReportId,
                ReporterName = r.ReporterName,
                TargetType = r.TargetType,
                TargetId = r.TargetId,
                TargetTitle = r.TargetTitle,
                Reason = r.Reason,
                Description = r.Description,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ResolvedAt = r.ResolvedAt
            }).ToList();
        }
    }
}