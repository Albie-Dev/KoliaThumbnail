using System.Text.Json;
using FluentValidation;
using Kolia.Thumbnail.API.Models.AIs;

namespace Kolia.Thumbnail.API.Validations.AIs
{
    /// <summary>
    /// Validator dùng chung cho các DTO của AI Configuration.
    /// </summary>
    public class AIConfigurationBaseValidator<T> : AbstractValidator<T>
        where T : AIConfigurationBaseDto
    {
        public AIConfigurationBaseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên cấu hình không được để trống.")
                .MaximumLength(100)
                .WithMessage("Tên cấu hình không được vượt quá 100 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.");

            RuleFor(x => x.ApiKey)
                .NotEmpty()
                .WithMessage("API Key không được để trống.")
                .MaximumLength(2000)
                .WithMessage("API Key không được vượt quá 2000 ký tự.");

            RuleFor(x => x.BaseUrl)
                .NotEmpty()
                .WithMessage("Base URL không được để trống.")
                .MaximumLength(500)
                .WithMessage("Base URL không được vượt quá 500 ký tự.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Base URL không hợp lệ.");

            RuleFor(x => x.Endpoint)
                .MaximumLength(500)
                .WithMessage("Endpoint không được vượt quá 500 ký tự.");

            RuleFor(x => x.ApiVersion)
                .MaximumLength(100)
                .WithMessage("API Version không được vượt quá 100 ký tự.");

            RuleFor(x => x.TimeoutSeconds)
                .InclusiveBetween(1, 600)
                .WithMessage("Timeout phải nằm trong khoảng từ 1 đến 600 giây.");

            RuleFor(x => x.RetryCount)
                .InclusiveBetween(0, 10)
                .WithMessage("Số lần thử lại phải nằm trong khoảng từ 0 đến 10.");

            RuleFor(x => x.Priority)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Độ ưu tiên phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.ExtraSettingsJson)
                .Must(BeValidJson)
                .WithMessage("Extra Settings phải là chuỗi JSON hợp lệ.");

            RuleFor(x => x.AIProviderId)
                .NotEmpty()
                .WithMessage("AI Provider không hợp lệ.");
        }

        private static bool BeValidJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

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

    /// <summary>
    /// Validator cho yêu cầu tạo mới AI Configuration.
    /// </summary>
    public sealed class AIConfigurationCreateValidator
        : AIConfigurationBaseValidator<AIConfiurationCreateDto>
    {
    }

    /// <summary>
    /// Validator cho yêu cầu cập nhật AI Configuration.
    /// </summary>
    public sealed class AIConfigurationUpdateValidator
        : AIConfigurationBaseValidator<AIConfigurationUpdateDto>
    {
    }
}