using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentalsAndProperties.Web.ViewModels.Property;

public class PropertySearchResultViewModel
{
    public List<PropertyCardViewModel> Properties { get; set; } = new();

    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    public PropertySearchQueryViewModel Query { get; set; } = new();
    public List<SelectListItem> Cities { get; set; } = new();
    public List<SelectListItem> ListingTypes { get; set; } = new();
    public List<SelectListItem> BhkTypes { get; set; } = new();
    public List<SelectListItem> FurnishingTypes { get; set; } = new();
    public bool IsAuthenticated { get; set; }
}