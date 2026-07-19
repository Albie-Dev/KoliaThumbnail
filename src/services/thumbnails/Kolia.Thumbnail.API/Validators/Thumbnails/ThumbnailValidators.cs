using FluentValidation;
using Kolia.Thumbnail.API.DTOs.Thumbnails;

namespace Kolia.Thumbnail.API.Validators.Thumbnails
{
    public class ThumbnailSearchRequestValidator : AbstractValidator<ThumbnailSearchRequest>
    {
        public ThumbnailSearchRequestValidator()
        {
            RuleFor(x => x.Keyword)
                .NotEmpty().WithMessage("Keyword không được để trống.")
                .MaximumLength(300);

            RuleFor(x => x.TimeFilter)
                .IsInEnum().WithMessage("Bộ lọc thời gian không hợp lệ.");

            RuleFor(x => x.SortFilter)
                .IsInEnum().WithMessage("Bộ lọc sắp xếp không hợp lệ.");
        }
    }

    public class ThumbnailManualImportRequestValidator : AbstractValidator<ThumbnailManualImportRequest>
    {
        public ThumbnailManualImportRequestValidator()
        {
            RuleFor(x => x.VideoUrl)
                .NotEmpty().WithMessage("URL video không được để trống.")
                .MaximumLength(1000)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("URL video không hợp lệ.");
        }
    }
}
