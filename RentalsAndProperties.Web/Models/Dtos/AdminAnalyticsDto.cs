namespace RentalsAndProperties.Web.Models.Dtos
{
    public class AdminAnalyticsDto
    {
        public int TotalUsers { get; set; }

        public int TotalOwners { get; set; }

        public int TotalProperties { get; set; }

        public int PendingApprovals { get; set; }

        public int CompletedTransactions { get; set; }

        public int TotalReviews { get; set; }

        public int TotalReports { get; set; }

        public int ActiveListings { get; set; }

        public decimal AveragePropertyPrice { get; set; }
        public List<ReportResponseDto> RecentReports { get; set; } = new();
    }
}