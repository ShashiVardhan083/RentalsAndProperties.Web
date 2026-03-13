using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RentalsAndProperties.Web.Models.Dtos;

namespace RentalsAndProperties.Web.ViewModels.Property
{
    public class UploadImagesViewModel
    {
        public Guid PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public List<PropertyImageDto> ExistingImages { get; set; } = new();
        public int MaxImages { get; set; } = 10;

        [Display(Name = "Property Images")]
        public List<IFormFile>? Images { get; set; }
    }
}