using FluentValidation;
using Kolia.Thumbnail.API.DTOs.Briefs;
using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Validators.Briefs
{
    public class SaveManualBriefRequestValidator : AbstractValidator<SaveManualBriefRequest>
    {
        public SaveManualBriefRequestValidator()
        {
            RuleFor(x => x.OverviewInput)
                .NotEmpty().WithMessage("Tổng quan livestream không được để trống.");

            RuleFor(x => x.ViewpointInput)
                .NotEmpty().WithMessage("Quan điểm không được để trống.");

            RuleFor(x => x.KeyDataInput)
                .NotEmpty().WithMessage("Dữ liệu quan trọng không được để trống.");
        }
    }

    public class ImportBriefRequestValidator : AbstractValidator<ImportBriefRequest>
    {
        public ImportBriefRequestValidator()
        {
            RuleFor(x => x.Source)
                .IsInEnum().WithMessage("Loại nguồn nhập không hợp lệ.");

            When(x => x.Source == CImportContentSource.PasteText, () =>
            {
                RuleFor(x => x.RawText)
                    .NotEmpty().WithMessage("Nội dung paste không được để trống.");
            });

            When(x => x.Source == CImportContentSource.File, () =>
            {
                RuleFor(x => x.FileUrl)
                    .NotEmpty().WithMessage("URL file không được để trống.")
                    .MaximumLength(1000);
            });

            When(x => x.Source == CImportContentSource.ExternalLink, () =>
            {
                RuleFor(x => x.ExternalLink)
                    .NotEmpty().WithMessage("Link ngoài không được để trống.")
                    .MaximumLength(1000);
            });
        }
    }
}
