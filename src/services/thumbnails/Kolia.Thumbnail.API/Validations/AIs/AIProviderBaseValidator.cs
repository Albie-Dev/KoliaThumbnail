using FluentValidation;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Validations.AIs
{
    /// <summary>
    /// Validator dùng chung cho các DTO của AI Provider.
    /// </summary>
    public class AIProviderBaseValidator<T> : AbstractValidator<T>
        where T : AIProviderBaseDto
    {
        public AIProviderBaseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên nhà cung cấp AI không được để trống.")
                .MaximumLength(100)
                .WithMessage("Tên nhà cung cấp AI không được vượt quá 100 ký tự.");

            RuleFor(x => x.ShortName)
                .NotEmpty()
                .WithMessage("Tên viết tắt không được để trống.")
                .MaximumLength(50)
                .WithMessage("Tên viết tắt không được vượt quá 50 ký tự.")
                .Matches("^[a-zA-Z0-9_-]+$")
                .WithMessage("Tên viết tắt chỉ được chứa chữ cái, số, dấu gạch ngang (-) và dấu gạch dưới (_).");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .WithMessage("Đường dẫn hình ảnh không được vượt quá 500 ký tự.")
                .Must(uri =>
                    string.IsNullOrWhiteSpace(uri) ||
                    Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Đường dẫn hình ảnh không hợp lệ.");
        }
    }


    /// <summary>
    /// Validator cho yêu cầu tạo mới AI Provider.
    /// </summary>
    public sealed class AIProviderCreateValidator
        : AIProviderBaseValidator<AIProviderCreateDto>
    {
    }

    /// <summary>
    /// Validator cho yêu cầu cập nhật AI Provider.
    /// </summary>
    public sealed class AIProviderUpdateValidator
        : AIProviderBaseValidator<AIProviderUpdateDto>
    {
        public AIProviderUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id nhà cung cấp AI không hợp lệ.");
        }
    }
}