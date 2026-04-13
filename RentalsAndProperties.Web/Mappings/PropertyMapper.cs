using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.Models.Enums;
using RentalsAndProperties.Web.ViewModels.Property;
using RentalsAndProperties.Web.ViewModels.Review;

namespace RentalsAndProperties.Web.Mappings
{
    public static class PropertyMapper
    {
        public static List<PropertyDetailViewModel> ToViewModel(List<PropertyDetailDto> dtoList)
        {
            return dtoList.Select(p => new PropertyDetailViewModel
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                Description = p.Description,
                City = p.City,
                Address = p.Address,
                Pincode = p.Pincode!,
                Price = p.Price,
                SecurityDeposit = p.SecurityDeposit,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                SquareFeet = (int)p.SquareFeet,
                PropertyType = p.PropertyType,
                ListingType = p.ListingType,
                BHKType = p.BHKType,
                FurnishingType = p.FurnishingType,
                AvailableFrom = p.AvailableFrom,
                Status = p.Status,
                OwnerName = p.OwnerName,
                OwnerPhone = p.OwnerPhone,
                Images = p.Images.Select(i => new PropertyImageViewModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList()
            }).ToList();
        }

        // Details
        public static PropertyDetailViewModel ToDetailViewModel(PropertyDetailDto p, List<ReviewResponseDto> reviews)
        {
            return new PropertyDetailViewModel
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                Description = p.Description,
                City = p.City,
                Address = p.Address,
                Pincode = p.Pincode!,
                Price = p.Price,
                SecurityDeposit = p.SecurityDeposit,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                SquareFeet = (int)p.SquareFeet,
                PropertyType = p.PropertyType,
                ListingType = p.ListingType,
                BHKType = p.BHKType,
                FurnishingType = p.FurnishingType,
                AvailableFrom = p.AvailableFrom,
                Status = p.Status,
                OwnerName = p.OwnerName,
                OwnerPhone = p.OwnerPhone,
                OwnerId = p.OwnerId,

                Images = p.Images.Select(i => new PropertyImageViewModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList(),

                Reviews = reviews.Select(r => new ReviewDisplayViewModel
                {
                    ReviewerName = r.ReviewerName,
                    CreatedAt = r.CreatedAt,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewType = r.ReviewType
                }).ToList()
            };
        }

        // Owner Dashboard
        public static List<PropertyCardViewModel> ToOwnerCardViewModel(List<PropertyResponseDto> dtoList)
        {
            return dtoList.Select(p => new PropertyCardViewModel
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                City = p.City,
                Price = p.Price,
                ListingType = p.ListingType,
                PropertyType = p.PropertyType,
                BHKType = p.BHKType,
                PrimaryImageUrl = p.PrimaryImageUrl,
                ImageUrls = p.ImageUrls ?? new List<string>(),
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                SquareFeet = (int)p.SquareFeet
            }).ToList();
        }
        public static List<PropertyCardViewModel> ToCardViewModel(List<PropertyCardDto> dtoList)
        {
            return dtoList.Select(property => new PropertyCardViewModel
            {
                FurnishingType = property.FurnishingType,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                SquareFeet = (int)property.SquareFeet,
                OwnerName = property.OwnerName,
                ImageUrls = property.ImageUrls,
                AverageRating = property.AverageRating,
                ReviewCount = property.ReviewCount,

                PropertyId = property.PropertyId,
                Title = property.Title,
                City = property.City,
                Price = property.Price,
                ListingType = property.ListingType,
                PropertyType = property.PropertyType,
                BHKType = property.BHKType,
                PrimaryImageUrl = property.PrimaryImageUrl
            }).ToList();
        }

        public static EditPropertyViewModel ToEditViewModel(PropertyDetailDto detail)
        {
            var editPropertyViewModel = new EditPropertyViewModel
            {
                PropertyId = detail.PropertyId,
                Title = detail.Title,
                Description = detail.Description,
                City = Enum.TryParse<CityEnum>(detail.City, true, out var city)
                    ? city
                    : CityEnum.Hyderabad,

                Address = detail.Address,
                Price = detail.Price,
                SecurityDeposit = detail.SecurityDeposit,
                SquareFeet = detail.SquareFeet,
                Bedrooms = detail.Bedrooms,
                Bathrooms = detail.Bathrooms,
                AvailableFrom = detail.AvailableFrom,
                CurrentStatus = detail.Status,

                ExistingImages = detail.Images.Select(i => new PropertyImageViewModel
                {
                    ImageId = i.ImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList(),

                Pincode = detail.Pincode ?? ""
            };

            // Enums
            if (Enum.TryParse<PropertyTypeEnum>(detail.PropertyType, true, out var pt))
                editPropertyViewModel.PropertyType = pt;

            if (Enum.TryParse<ListingTypeEnum>(detail.ListingType, true, out var lt))
                editPropertyViewModel.ListingType = lt;

            if (Enum.TryParse<FurnishingTypeEnum>(detail.FurnishingType, true, out var ft))
                editPropertyViewModel.FurnishingType = ft;

            // BHK Mapping
            editPropertyViewModel.BHKType = detail.BHKType switch
            {
                "1 RK" => BhkTypeEnum.OneRK,
                "1 BHK" => BhkTypeEnum.OneBHK,
                "2 BHK" => BhkTypeEnum.TwoBHK,
                "3 BHK" => BhkTypeEnum.ThreeBHK,
                "4 BHK" => BhkTypeEnum.FourBHK,
                "Penthouse" => BhkTypeEnum.Penthouse,
                _ => BhkTypeEnum.OneBHK
            };

            return editPropertyViewModel;
        }
    }
}