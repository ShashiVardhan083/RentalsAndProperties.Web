using RentalsAndProperties.Web.ViewModels.Report;

namespace RentalsAndProperties.Web.ViewModels.Admin
{
    public class AdminAnalyticsViewModel
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
        public List<CityStatViewModel> TopCities { get; set; } = new();
        public List<MonthlyStatViewModel> MonthlyRegistrations { get; set; } = new();
        public List<MonthlyStatViewModel> MonthlyTransactions { get; set; } = new();
        public List<ReportViewModel> RecentReports { get; set; } = new();
    }

    public class CityStatViewModel
    {
        public string City { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MonthlyStatViewModel
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}