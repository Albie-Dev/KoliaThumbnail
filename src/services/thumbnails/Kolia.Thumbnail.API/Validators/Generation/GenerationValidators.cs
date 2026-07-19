using FluentValidation;
using Kolia.Thumbnail.API.DTOs.DisplayTexts;
using Kolia.Thumbnail.API.DTOs.ThumbnailGeneration;
using Kolia.Thumbnail.API.DTOs.VideoTitles;

namespace Kolia.Thumbnail.API.Validators.Generation
{
    public class GenerateDisplayTextRequestValidator : AbstractValidator<GenerateDisplayTextRequest>
    {
        public GenerateDisplayTextRequestValidator()
        {
            RuleFor(x => x.NewsItemIds)
                .NotEmpty().WithMessage("Phải chọn ít nhất 1 bản tin.")
                .Must(ids => ids.Count <= 10).WithMessage("Tối đa 10 bản tin mỗi lần generate.");
        }
    }

    public class GenerateThumbnailRequestValidator : AbstractValidator<GenerateThumbnailRequest>
    {
        private static readonly HashSet<string> ValidRatios = ["16:9", "1:1", "9:16", "4:3"];
        private static readonly HashSet<string> ValidResolutions = ["1K", "2K", "4K"];

        public GenerateThumbnailRequestValidator()
        {
            RuleFor(x => x.DisplayTextOptionIds)
                .NotEmpty().WithMessage("Phải chọn ít nhất 1 Display Text option.");

            RuleFor(x => x.RequestedCount)
                .InclusiveBetween(1, 5).WithMessage("Số ảnh yêu cầu phải từ 1 đến 5.");

            RuleFor(x => x.Ratio)
                .NotEmpty()
                .Must(r => ValidRatios.Contains(r)).WithMessage($"Tỷ lệ phải là một trong: {string.Join(", ", ValidRatios)}");

            RuleFor(x => x.Resolution)
                .NotEmpty()
                .Must(r => ValidResolutions.Contains(r)).WithMessage($"Độ phân giải phải là: {string.Join(", ", ValidResolutions)}");
        }
    }

    public class GenerateVideoTitleRequestValidator : AbstractValidator<GenerateVideoTitleRequest>
    {
        private static readonly HashSet<int> ValidCounts = [3, 5, 7, 10];

        public GenerateVideoTitleRequestValidator()
        {
            RuleFor(x => x.SelectedThumbnailIds)
                .NotEmpty().WithMessage("Phải chọn ít nhất 1 thumbnail.");

            RuleFor(x => x.SelectedNewsItemIds)
                .NotEmpty().WithMessage("Phải chọn ít nhất 1 bản tin.");

            RuleFor(x => x.Style)
                .IsInEnum().WithMessage("Phong cách viết title không hợp lệ.");

            RuleFor(x => x.RequestedCount)
                .Must(c => ValidCounts.Contains(c))
                .WithMessage($"Số title phải là một trong: {string.Join(", ", ValidCounts)}");
        }
    }

    public class VideoTitleFeedbackRequestValidator : AbstractValidator<VideoTitleFeedbackRequest>
    {
        public VideoTitleFeedbackRequestValidator()
        {
            RuleFor(x => x.FeedbackText)
                .NotEmpty().WithMessage("Feedback không được để trống.")
                .MaximumLength(1000);
        }
    }
}
