namespace RentalsAndProperties.Web.ViewModels.Review
{
    public class ReviewDisplayViewModel
    {
        public string ReviewerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        public string ReviewType { get; set; } = string.Empty;
    }
}
