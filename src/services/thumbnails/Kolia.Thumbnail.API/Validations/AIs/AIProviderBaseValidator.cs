using System.Text.Json;
using FluentValidation;
using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Enums;
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

            RuleFor(x => x.ProviderType)
                .IsInEnum()
                .WithMessage("Loại nhà cung cấp AI không hợp lệ.")
                .NotEqual(CAIProviderType.System)
                .WithMessage("Không thể chọn loại nhà cung cấp AI là System.");

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

#if FALSE
            RuleForEach(x => x.Endpoints)
                .SetValidator(new AIProviderEndpointValidator());
#endif
        }
    }

#if FALSE
    public class AIProviderEndpointValidator : AbstractValidator<AIProviderEndpoint>
    {
        public AIProviderEndpointValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Loại endpoint không hợp lệ.");

            RuleFor(x => x.Route)
                .NotEmpty()
                .WithMessage("Route không được để trống.")
                .MaximumLength(500)
                .WithMessage("Route không được vượt quá 500 ký tự.");

            RuleFor(x => x.JsonResponse)
                .NotEmpty()
                .WithMessage("JsonResponse không được để trống.")
                .Must(BeValidJson)
                .WithMessage("JsonResponse không phải JSON hợp lệ.");

            RuleFor(x => x.JsonError)
                .NotEmpty()
                .WithMessage("JsonError không được để trống.")
                .Must(BeValidJson)
                .WithMessage("JsonError không phải JSON hợp lệ.");

            RuleFor(x => x.JsonRequest)
                .Must(x => string.IsNullOrWhiteSpace(x) || BeValidJson(x))
                .WithMessage("JsonRequest không phải JSON hợp lệ.");
        }

        private static bool BeValidJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
#endif


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