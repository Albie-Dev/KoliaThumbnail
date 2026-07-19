using FluentValidation;
using Kolia.Thumbnail.API.DTOs.Projects;

namespace Kolia.Thumbnail.API.Validators.Projects
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên project không được để trống.")
                .MaximumLength(200).WithMessage("Tên project tối đa 200 ký tự.");
        }
    }

    public class RenameProjectRequestValidator : AbstractValidator<RenameProjectRequest>
    {
        public RenameProjectRequestValidator()
        {
            RuleFor(x => x.NewName)
                .NotEmpty().WithMessage("Tên mới không được để trống.")
                .MaximumLength(200).WithMessage("Tên project tối đa 200 ký tự.");
        }
    }
}
