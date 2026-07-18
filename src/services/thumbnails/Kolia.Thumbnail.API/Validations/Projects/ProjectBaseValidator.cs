using FluentValidation;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Models.Projects;

namespace Kolia.Thumbnail.API.Validations.Projects
{
    /// <summary>
    /// Validator dùng chung cho các DTO của Project.
    /// </summary>
    public class ProjectBaseValidator<T> : AbstractValidator<T>
        where T : ProjectBaseDto
    {
        public ProjectBaseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên dự án không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tên dự án không được vượt quá 200 ký tự.");

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Mã dự án không được để trống.")
                .MaximumLength(50)
                .WithMessage("Mã dự án không được vượt quá 50 ký tự.")
                .Matches("^[a-zA-Z0-9_-]+$")
                .WithMessage("Mã dự án chỉ được chứa chữ cái, số, dấu gạch ngang (-) và dấu gạch dưới (_).");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.");

            RuleFor(x => x.ErrorMessage)
                .MaximumLength(500)
                .WithMessage("Thông báo lỗi không được vượt quá 500 ký tự.");

            RuleFor(x => x.ErrorDetail)
                .MaximumLength(2000)
                .WithMessage("Chi tiết lỗi không được vượt quá 2000 ký tự.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Trạng thái dự án không hợp lệ.");

            RuleFor(x => x.Progress)
                .InclusiveBetween(0, 100)
                .WithMessage("Tiến độ phải nằm trong khoảng từ 0 đến 100.");

            RuleFor(x => x.TotalSteps)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tổng số bước không được âm.");

            RuleFor(x => x.CompletedSteps)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Số bước hoàn thành không được âm.");
        }
    }

    /// <summary>
    /// Validator cho yêu cầu tạo mới Project.
    /// Client chỉ gửi Name + Description; các field khác do backend tự set.
    /// </summary>
    public sealed class ProjectCreateValidator : AbstractValidator<ProjectCreateDto>
    {
        public ProjectCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tên dự án không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tên dự án không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Mô tả không được vượt quá 1000 ký tự.");
        }
    }

    /// <summary>
    /// Validator cho yêu cầu cập nhật Project.
    /// </summary>
    public sealed class ProjectUpdateValidator
        : ProjectBaseValidator<ProjectUpdateDto>
    {
        public ProjectUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id dự án không được để trống.");
        }
    }
}