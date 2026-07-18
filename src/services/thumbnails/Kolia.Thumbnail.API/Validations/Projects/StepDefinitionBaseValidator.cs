using FluentValidation;
using Kolia.Thumbnail.API.Models.Projects;

namespace Kolia.Thumbnail.API.Validations.Projects
{
    /// <summary>
    /// Validator dùng chung cho các DTO của StepDefinition.
    /// </summary>
    public class StepDefinitionBaseValidator<T> : AbstractValidator<T>
        where T : StepDefinitionBaseDto
    {
        public StepDefinitionBaseValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Mã định nghĩa bước không được để trống.")
                .MaximumLength(50)
                .WithMessage("Mã định nghĩa bước không được vượt quá 50 ký tự.")
                .Matches("^[a-zA-Z0-9_-]+$")
                .WithMessage("Mã định nghĩa bước chỉ được chứa chữ cái, số, dấu gạch ngang (-) và dấu gạch dưới (_).");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên định nghĩa bước không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tên định nghĩa bước không được vượt quá 200 ký tự.");

            RuleFor(x => x.DisplayCode)
                .NotEmpty()
                .WithMessage("Mã hiển thị không được để trống.")
                .MaximumLength(10)
                .WithMessage("Mã hiển thị không được vượt quá 10 ký tự.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Thứ tự sắp xếp không được âm.");
        }
    }

    /// <summary>
    /// Validator cho yêu cầu tạo mới StepDefinition.
    /// </summary>
    public sealed class StepDefinitionCreateValidator
        : StepDefinitionBaseValidator<StepDefinitionCreateDto>
    {
    }

    /// <summary>
    /// Validator cho yêu cầu cập nhật StepDefinition.
    /// </summary>
    public sealed class StepDefinitionUpdateValidator
        : StepDefinitionBaseValidator<StepDefinitionUpdateDto>
    {
        public StepDefinitionUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id định nghĩa bước không được để trống.");
        }
    }
}