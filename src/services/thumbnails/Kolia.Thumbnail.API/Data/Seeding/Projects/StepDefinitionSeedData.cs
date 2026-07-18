using Kolia.Thumbnail.API.Data.Entities.Projects;

namespace Kolia.Thumbnail.API.Data.Seeding.Projects
{
    public static class StepDefinitionSeedData
    {
        public static IEnumerable<StepDefinitionEntity> GetAll() => new[]
        {
            new StepDefinitionEntity(WellKnownStepDefinitionIds.VideoContent)
            {
                Code = "video_content", Name = "Nội dung video",
                DisplayCode = "1", SortOrder = 1, ParentId = null, IsTrackable = true,
            },
            new StepDefinitionEntity(WellKnownStepDefinitionIds.News)
            {
                Code = "news", Name = "Tin tức",
                DisplayCode = "2", SortOrder = 2, ParentId = null, IsTrackable = true,
            },
            new StepDefinitionEntity(WellKnownStepDefinitionIds.ThumbnailReference)
            {
                Code = "thumbnail_reference", Name = "Thumbnail tham khảo",
                DisplayCode = "3", SortOrder = 3, ParentId = null,
                IsTrackable = false, // chỉ là nhóm UI, không có ProjectStep riêng
            },
            new StepDefinitionEntity(WellKnownStepDefinitionIds.ThumbnailReferenceLibrary)
            {
                Code = "thumbnail_reference_library", Name = "Thumbnail library",
                DisplayCode = "3.1", SortOrder = 1,
                ParentId = WellKnownStepDefinitionIds.ThumbnailReference, IsTrackable = true,
            },
            new StepDefinitionEntity(WellKnownStepDefinitionIds.Thumbnail)
            {
                Code = "thumbnail", Name = "Thumbnail",
                DisplayCode = "4", SortOrder = 4, ParentId = null,
                IsTrackable = false,
            },
            new StepDefinitionEntity(WellKnownStepDefinitionIds.ThumbnailDisplayText)
            {
                Code = "thumbnail_display_text", Name = "Tạo display text",
                DisplayCode = "4.1", SortOrder = 1,
                ParentId = WellKnownStepDefinitionIds.Thumbnail, IsTrackable = true,
            },
            new StepDefinitionEntity(WellKnownStepDefinitionIds.ThumbnailGenerate)
            {
                Code = "thumbnail_generate", Name = "Tạo thumbnail",
                DisplayCode = "4.2", SortOrder = 2,
                ParentId = WellKnownStepDefinitionIds.Thumbnail, IsTrackable = true,
            },
            new StepDefinitionEntity(WellKnownStepDefinitionIds.VideoTitle)
            {
                Code = "video_title", Name = "Tạo video title",
                DisplayCode = "5", SortOrder = 5, ParentId = null, IsTrackable = true,
            },
            new StepDefinitionEntity(WellKnownStepDefinitionIds.CompleteSet)
            {
                Code = "complete_set", Name = "Bộ hoàn chỉnh",
                DisplayCode = "6", SortOrder = 6, ParentId = null, IsTrackable = true,
            },
        };
    }

    /// <summary>
    /// GUID cố định cho từng StepDefinition — bắt buộc để HasData snapshot ổn định
    /// qua các lần migrate. Cũng dùng để code khác tham chiếu bằng hằng số thay vì
    /// query theo Code (tránh magic string rải rác).
    /// </summary>
    public static class WellKnownStepDefinitionIds
    {
        public static readonly Guid VideoContent = new("11111111-1111-1111-1111-111111111101");
        public static readonly Guid News = new("11111111-1111-1111-1111-111111111102");
        public static readonly Guid ThumbnailReference = new("11111111-1111-1111-1111-111111111103");
        public static readonly Guid ThumbnailReferenceLibrary = new("11111111-1111-1111-1111-111111111131");
        public static readonly Guid Thumbnail = new("11111111-1111-1111-1111-111111111104");
        public static readonly Guid ThumbnailDisplayText = new("11111111-1111-1111-1111-111111111141");
        public static readonly Guid ThumbnailGenerate = new("11111111-1111-1111-1111-111111111142");
        public static readonly Guid VideoTitle = new("11111111-1111-1111-1111-111111111105");
        public static readonly Guid CompleteSet = new("11111111-1111-1111-1111-111111111106");
    }
}