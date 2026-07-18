using FluentValidation;
using Kolia.Thumbnail.API.Models.Projects;

namespace Kolia.Thumbnail.API.Validations.Projects
{
    /// <summary>
    /// Validator dùng chung cho các DTO của ProjectStep.
    /// </summary>
    public class ProjectStepBaseValidator<T> : AbstractValidator<T>
        where T : ProjectStepBaseDto
    {
        public ProjectStepBaseValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Trạng thái bước không hợp lệ.");

            RuleFor(x => x.ContentJson)
                .MaximumLength(5000)
                .WithMessage("Nội dung JSON không được vượt quá 5000 ký tự.")
                .Must(BeValidJson)
                .When(x => !string.IsNullOrWhiteSpace(x.ContentJson))
                .WithMessage("Nội dung JSON không phải là JSON hợp lệ.");

            RuleFor(x => x.ErrorMessage)
                .MaximumLength(1000)
                .WithMessage("Thông báo lỗi không được vượt quá 1000 ký tự.");
        }

        private static bool BeValidJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return true;

            try
            {
                System.Text.Json.JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Validator cho yêu cầu tạo mới ProjectStep.
    /// </summary>
    public sealed class ProjectStepCreateValidator
        : ProjectStepBaseValidator<ProjectStepCreateDto>
    {
        public ProjectStepCreateValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("Id dự án không được để trống.");

            RuleFor(x => x.StepDefinitionId)
                .NotEmpty()
                .WithMessage("Id định nghĩa bước không được để trống.");
        }
    }

    /// <summary>
    /// Validator cho yêu cầu cập nhật ProjectStep.
    /// </summary>
    public sealed class ProjectStepUpdateValidator
        : ProjectStepBaseValidator<ProjectStepUpdateDto>
    {
        public ProjectStepUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id bước thực hiện không được để trống.");
        }
    }
}