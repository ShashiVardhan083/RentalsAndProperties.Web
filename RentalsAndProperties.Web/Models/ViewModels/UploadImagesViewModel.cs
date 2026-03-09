using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RentalsAndProperties.Web.Models;

namespace RentalsAndProperties.Web.Models.ViewModels
{
    public class UploadImagesViewModel
    {
        public Guid PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public List<PropertyImageModel> ExistingImages { get; set; } = new();
        public int MaxImages { get; set; } = 10;

        [Display(Name = "Property Images")]
        public List<IFormFile>? Images { get; set; }
    }
}