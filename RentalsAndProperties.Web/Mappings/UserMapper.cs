using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.ViewModels.Admin;

namespace RentalsAndProperties.Web.Mappings
{
    public static class UserMapper
    {
        public static List<UserListViewModel> ToViewModel(List<UserDto> UserDtoList)
        {
            return UserDtoList.Select(u => new UserListViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                Roles = string.Join(", ", u.Roles),
                Status = u.IsBlocked ? "Blocked" : "Active",
                CreatedAt = u.CreatedAt.ToString("dd MMM yyyy")
            }).ToList();
        }
    }
}