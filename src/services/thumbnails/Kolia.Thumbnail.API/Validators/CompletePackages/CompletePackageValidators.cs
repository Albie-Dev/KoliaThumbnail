using FluentValidation;
using Kolia.Thumbnail.API.DTOs.CompletePackages;

namespace Kolia.Thumbnail.API.Validators.CompletePackages
{
    public class ConfirmPackageRequestValidator : AbstractValidator<ConfirmPackageRequest>
    {
        public ConfirmPackageRequestValidator()
        {
            RuleFor(x => x.SelectedThumbnailId)
                .NotEmpty().WithMessage("Phải chọn 1 thumbnail.");

            RuleFor(x => x.SelectedTitleOptionIds)
                .NotEmpty().WithMessage("Phải chọn ít nhất 1 title.");
        }
    }
}
