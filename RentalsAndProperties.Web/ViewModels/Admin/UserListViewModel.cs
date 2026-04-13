namespace RentalsAndProperties.Web.ViewModels.Admin
{
    public class UserListViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string? Email { get; set; }
        public string Roles { get; set; } = "";
        public string Status { get; set; } = ""; 
        public string CreatedAt { get; set; } = "";
    }
}