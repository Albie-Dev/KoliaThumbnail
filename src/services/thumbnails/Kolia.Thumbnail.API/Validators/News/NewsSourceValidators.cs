using FluentValidation;
using Kolia.Thumbnail.API.DTOs.News;

namespace Kolia.Thumbnail.API.Validators.News
{
    public sealed class NewsSourceCreateDtoValidator : AbstractValidator<NewsSourceCreateDto>
    {
        public NewsSourceCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên nguồn tin không được để trống.")
                .MaximumLength(200);

            RuleFor(x => x.RssOrFeedUrl)
                .NotEmpty().WithMessage("URL RSS/Feed không được để trống.")
                .Must(BeAValidUrl).WithMessage("URL RSS/Feed không hợp lệ. Phải bắt đầu bằng http:// hoặc https://.")
                .MaximumLength(1000);

            RuleFor(x => x.Domain)
                .NotEmpty().WithMessage("Domain không được để trống.")
                .MaximumLength(200)
                .Must((dto, domain) => DomainMatchesUrl(dto.RssOrFeedUrl, domain))
                .WithMessage("Domain phải khớp với host trong RssOrFeedUrl (tránh admin gõ nhầm domain dùng cho rate-limiter).");

            RuleFor(x => x.Priority)
                .GreaterThanOrEqualTo(0).WithMessage("Priority phải >= 0.");

            RuleFor(x => x.SourceGroup)
                .IsInEnum().WithMessage("SourceGroup không hợp lệ.");

            RuleFor(x => x.FetchMode)
                .IsInEnum().WithMessage("FetchMode không hợp lệ.");

            RuleFor(x => x.Region)
                .IsInEnum().WithMessage("Region không hợp lệ.");
        }

        private static bool BeAValidUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        private static bool DomainMatchesUrl(string url, string domain)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return true;
            var host = uri.Host.ToLowerInvariant();
            var normalised = domain.ToLowerInvariant().TrimStart('.');
            return host == normalised
                || host.EndsWith("." + normalised)
                || normalised.EndsWith("." + host);
        }
    }

    public sealed class NewsSourceUpdateDtoValidator : AbstractValidator<NewsSourceUpdateDto>
    {
        public NewsSourceUpdateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên nguồn tin không được để trống.")
                .MaximumLength(200);

            RuleFor(x => x.RssOrFeedUrl)
                .NotEmpty().WithMessage("URL RSS/Feed không được để trống.")
                .Must(BeAValidUrl).WithMessage("URL RSS/Feed không hợp lệ. Phải bắt đầu bằng http:// hoặc https://.")
                .MaximumLength(1000);

            RuleFor(x => x.Domain)
                .NotEmpty().WithMessage("Domain không được để trống.")
                .MaximumLength(200)
                .Must((dto, domain) => DomainMatchesUrl(dto.RssOrFeedUrl, domain))
                .WithMessage("Domain phải khớp với host trong RssOrFeedUrl (tránh admin gõ nhầm domain dùng cho rate-limiter).");

            RuleFor(x => x.Priority)
                .GreaterThanOrEqualTo(0).WithMessage("Priority phải >= 0.");

            RuleFor(x => x.SourceGroup)
                .IsInEnum().WithMessage("SourceGroup không hợp lệ.");

            RuleFor(x => x.FetchMode)
                .IsInEnum().WithMessage("FetchMode không hợp lệ.");

            RuleFor(x => x.Region)
                .IsInEnum().WithMessage("Region không hợp lệ.");
        }

        private static bool BeAValidUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        private static bool DomainMatchesUrl(string url, string domain)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return true;
            var host = uri.Host.ToLowerInvariant();
            var normalised = domain.ToLowerInvariant().TrimStart('.');
            return host == normalised
                || host.EndsWith("." + normalised)
                || normalised.EndsWith("." + host);
        }
    }
}
