using FluentValidation;
using Kolia.Thumbnail.API.DTOs.News;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Validators.News
{
    public class NewsSearchRequestValidator : AbstractValidator<NewsSearchRequest>
    {
        public NewsSearchRequestValidator()
        {
            RuleFor(x => x.MarketScope)
                .IsInEnum().WithMessage("Phạm vi thị trường không hợp lệ.");

            RuleFor(x => x.TimeRange)
                .IsInEnum().WithMessage("Khoảng thời gian không hợp lệ.");

            // Cảnh báo khi chọn Last30Days — hiệu năng cao
            RuleFor(x => x.TimeRange)
                .Must(t => t != CNewsTimeRange.Last30Days)
                .WithMessage("Last30Days có thể chậm vì lượng tin rất lớn. Hãy cân nhắc dùng Last7Days.")
                .WithSeverity(Severity.Warning);

            RuleFor(x => x.CountFilter)
                .IsInEnum().WithMessage("Bộ lọc số lượng không hợp lệ.");

            RuleFor(x => x.KeywordsRaw)
                .NotEmpty().WithMessage("Keyword không được để trống.")
                .MaximumLength(1000);
        }
    }

    public class NewsManualImportRequestValidator : AbstractValidator<NewsManualImportRequest>
    {
        public NewsManualImportRequestValidator()
        {
            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("URL không được để trống.")
                .MaximumLength(1000)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("URL không hợp lệ.");
        }
    }
}
