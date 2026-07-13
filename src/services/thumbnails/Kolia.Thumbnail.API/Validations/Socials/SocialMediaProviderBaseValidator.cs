using FluentValidation;
using Kolia.Thumbnail.API.Models.SocialMedias;

namespace Kolia.Thumbnail.API.Validations.Socials
{
    /// <summary>
    /// Validator dùng chung cho các DTO của Social Media Provider.
    /// </summary>
    public class SocialMediaProviderBaseValidator<T> : AbstractValidator<T>
        where T : SocialMediaProviderBaseDto
    {
        public SocialMediaProviderBaseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên nhà cung cấp Social Media không được để trống.")
                .MaximumLength(100)
                .WithMessage("Tên nhà cung cấp Social Media không được vượt quá 100 ký tự.");

            RuleFor(x => x.ShortName)
                .NotEmpty()
                .WithMessage("Tên viết tắt không được để trống.")
                .MaximumLength(50)
                .WithMessage("Tên viết tắt không được vượt quá 50 ký tự.")
                .Matches("^[a-zA-Z0-9_-]+$")
                .WithMessage("Tên viết tắt chỉ được chứa chữ cái, số, dấu gạch ngang (-) và dấu gạch dưới (_).");

            RuleFor(x => x.ProviderType)
                .IsInEnum()
                .WithMessage("Loại nhà cung cấp Social Media không hợp lệ.");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .WithMessage("Đường dẫn hình ảnh không được vượt quá 500 ký tự.")
                .Must(uri =>
                    string.IsNullOrWhiteSpace(uri) ||
                    Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Đường dẫn hình ảnh không hợp lệ.");

            RuleFor(x => x.BaseUrl)
                .NotEmpty()
                .WithMessage("URL cơ sở không được để trống.")
                .MaximumLength(500)
                .WithMessage("URL cơ sở không được vượt quá 500 ký tự.")
                .Must(uri =>
                    Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("URL cơ sở không hợp lệ.");
        }
    }


    /// <summary>
    /// Validator cho yêu cầu tạo mới Social Media Provider.
    /// </summary>
    public sealed class SocialMediaProviderCreateValidator
        : SocialMediaProviderBaseValidator<SocialMediaProviderCreateDto>
    {
    }

    /// <summary>
    /// Validator cho yêu cầu cập nhật Social Media Provider.
    /// </summary>
    public sealed class SocialMediaProviderUpdateValidator
        : SocialMediaProviderBaseValidator<SocialMediaProviderUpdateDto>
    {
        public SocialMediaProviderUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id nhà cung cấp Social Media không hợp lệ.");
        }
    }
}