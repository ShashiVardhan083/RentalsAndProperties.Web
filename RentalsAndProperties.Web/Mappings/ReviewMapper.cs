using RentalsAndProperties.Web.Models.Dtos;
using RentalsAndProperties.Web.ViewModels.Review;

namespace RentalsAndProperties.Web.Mappings
{
    public static class ReviewMapper
    {
        public static CreateReviewRequestDto ToCreateDto(CreateReviewViewModel createReviewViewModel)
        {
            return new CreateReviewRequestDto
            {
                PropertyId = createReviewViewModel.PropertyId,
                ReviewType = createReviewViewModel.ReviewType,
                Rating = createReviewViewModel.Rating,
                Comment = createReviewViewModel.Comment,
                OwnerResponsiveness = createReviewViewModel.OwnerResponsiveness,
                PropertyAccuracy = createReviewViewModel.PropertyAccuracy,
                TransactionId = createReviewViewModel.TransactionId
            };
        }
    }
}