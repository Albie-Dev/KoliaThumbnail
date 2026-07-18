using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Models.Projects
{
    /// <summary>
    /// DTO dùng để tạo mới một bước thực hiện trong dự án. Nó chứa các thông tin cơ bản cần thiết để định nghĩa một bước thực hiện trong hệ thống.
    /// </summary>
    public record ProjectStepCreateDto : ProjectStepBaseDto
    {
        /// <summary>
        /// Id của dự án chứa bước này.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Id của định nghĩa bước thực hiện.
        /// </summary>
        public Guid StepDefinitionId { get; set; }
    }

    /// <summary>
    /// DTO dùng để cập nhật thông tin của một bước thực hiện hiện có. Nó bao gồm các thông tin cơ bản cần thiết để xác định và cập nhật một bước thực hiện trong hệ thống.
    /// </summary>
    public record ProjectStepUpdateDto : ProjectStepBaseDto
    {
        /// <summary>
        /// Id của bước thực hiện cần được cập nhật. Đây là một giá trị duy nhất để xác định bước thực hiện trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// DTO dùng để trả về thông tin chi tiết của một bước thực hiện. Nó bao gồm các thông tin cơ bản cũng như Id duy nhất của bước thực hiện trong hệ thống.
    /// </summary>
    public record ProjectStepDetailDto : ProjectStepBaseDto
    {
        /// <summary>
        /// Id của bước thực hiện. Đây là một giá trị duy nhất để xác định bước thực hiện trong hệ thống.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id của dự án chứa bước này.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Id của định nghĩa bước thực hiện.
        /// </summary>
        public Guid StepDefinitionId { get; set; }

        /// <summary>
        /// Thông tin định nghĩa bước thực hiện.
        /// </summary>
        public StepDefinitionDetailDto? StepDefinition { get; set; }

        /// <summary>
        /// Trạng thái xóa của bước thực hiện. Nếu giá trị là true, bước thực hiện đã bị xóa; nếu là false, bước thực hiện vẫn còn tồn tại trong hệ thống.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Thời gian tạo của bước thực hiện. Đây là thời điểm mà bước thực hiện được thêm vào hệ thống.
        /// </summary>
        public DateTimeOffset? CreationTime { get; set; }

        /// <summary>
        /// Thời gian sửa đổi cuối cùng của bước thực hiện. Đây là thời điểm mà thông tin của bước thực hiện được cập nhật lần cuối cùng trong hệ thống.
        /// </summary>
        public DateTimeOffset? LastModificationTime { get; set; }

        /// <summary>
        /// Thời gian xóa của bước thực hiện. Nếu bước thực hiện đã bị xóa, đây là thời điểm mà nó được đánh dấu là đã xóa trong hệ thống. Nếu chưa bị xóa, giá trị này sẽ là null.
        /// </summary>
        public DateTimeOffset? DeletionTime { get; set; }
    }

    /// <summary>
    /// Node cây step để render UI: gộp cả bước "nhóm" (IsTrackable=false, vd bước 3, 4)
    /// lẫn bước "thực thi". Status của node nhóm được suy ra từ children.
    /// </summary>
    public record ProjectStepTreeNodeDto
    {
        public Guid StepDefinitionId { get; init; }
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string DisplayCode { get; init; } = null!;
        public bool IsTrackable { get; init; }
        public CProjectStepStatus Status { get; init; }
        public string? ContentJson { get; init; }
        public DateTimeOffset? StartedAt { get; init; }
        public DateTimeOffset? CompletedAt { get; init; }
        public string? ErrorMessage { get; init; }
        public List<ProjectStepTreeNodeDto> Children { get; init; } = new();
    }

    /// <summary>
    /// DTO dùng để đại diện cho các thuộc tính cơ bản của một bước thực hiện. Nó chứa các thông tin chung mà tất cả các DTO liên quan đến bước thực hiện đều có.
    /// </summary>
    public record ProjectStepBaseDto
    {
        /// <summary>
        /// Trạng thái của bước thực hiện.
        /// </summary>
        public CProjectStepStatus Status { get; set; }

        /// <summary>
        /// Nội dung của bước thực hiện dưới dạng JSON (linh hoạt cho từng loại bước).
        /// </summary>
        public string? ContentJson { get; set; }

        /// <summary>
        /// Thời gian bắt đầu thực hiện bước này.
        /// </summary>
        public DateTimeOffset? StartedAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành bước này.
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }

        /// <summary>
        /// Thông báo lỗi xảy ra ở bước này.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    public record DashboardStatisticsRequestDto
    {
        public int TrendDays { get; init; } = 14;
        public int RecentProjectsCount { get; init; } = 5;
    }

    public record ProjectDashboardStatisticsDto
    {
        public int TotalProjects { get; init; }
        public IReadOnlyDictionary<CProjectStatus, int> ProjectsByStatus { get; init; }
            = new Dictionary<CProjectStatus, int>();
        public double AverageProgress { get; init; }
        public int ProjectsCreatedToday { get; init; }
        public int ProjectsCompletedThisWeek { get; init; }
        public IReadOnlyList<StepBottleneckDto> StepBottlenecks { get; init; } = new List<StepBottleneckDto>();
        public IReadOnlyList<ProjectTrendPointDto> CreationTrend { get; init; } = new List<ProjectTrendPointDto>();
        public IReadOnlyList<ProjectDetailDto> RecentProjects { get; init; } = new List<ProjectDetailDto>();
    }

    public record StepBottleneckDto
    {
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public int FailedCount { get; init; }
        public int InProgressCount { get; init; }
        public double? AverageDurationMinutes { get; init; }
    }

    public record ProjectTrendPointDto
    {
        public DateOnly Date { get; init; }
        public int Count { get; init; }
    }

}