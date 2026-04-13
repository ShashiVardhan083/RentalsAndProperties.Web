using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.ViewModels.Admin;
using RentalsAndProperties.Web.ViewModels.Report;

namespace RentalsAndProperties.Web.Mappings
{
    public static class AnalyticsMapper
    {
        public static AdminAnalyticsViewModel ToViewModel(AdminAnalyticsDto adminAnalyticsDto)
        {
            return new AdminAnalyticsViewModel
            {
                TotalUsers = adminAnalyticsDto.TotalUsers,
                TotalOwners = adminAnalyticsDto.TotalOwners,
                TotalProperties = adminAnalyticsDto.TotalProperties,
                PendingApprovals = adminAnalyticsDto.PendingApprovals,
                ActiveListings = adminAnalyticsDto.ActiveListings,
                CompletedTransactions = adminAnalyticsDto.CompletedTransactions,
                TotalReviews = adminAnalyticsDto.TotalReviews,
                TotalReports = adminAnalyticsDto.TotalReports,
                AveragePropertyPrice = adminAnalyticsDto.AveragePropertyPrice,

                RecentReports = adminAnalyticsDto.RecentReports.Select(r => new ReportViewModel
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
                }).ToList()
            };
        }
    }
}