using System.Text.Json;

namespace Kolia.Thumbnail.API.Models.Commons
{
    /// <summary>
    /// DTO dùng để yêu cầu phân trang, lọc và sắp xếp dữ liệu.
    /// </summary>
    public class PagedRequestDto
    {
        /// <summary>
        /// Số trang cần lấy.
        /// Giá trị bắt đầu từ 1.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Số lượng bản ghi trên mỗi trang.
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Có trả về tổng số bản ghi hay không.
        /// Khi false sẽ tăng hiệu năng với dữ liệu lớn.
        /// </summary>
        public bool IncludeTotalCount { get; set; } = true;

        /// <summary>
        /// Có trả về dữ liệu hay chỉ lấy tổng số lượng.
        /// </summary>
        public bool IncludeItems { get; set; } = true;

        /// <summary>
        /// Văn bản tìm kiếm.
        /// </summary>
        public string? SearchText { get; set; }

        /// <summary>
        /// Danh sách điều kiện lọc.
        /// </summary>
        public List<FilterRequestDto> Filters { get; set; } = [];

        /// <summary>
        /// Danh sách điều kiện sắp xếp.
        /// </summary>
        public List<SortRequestDto> Sorts { get; set; } = [];
    }

    /// <summary>
    /// DTO đại diện cho một điều kiện lọc.
    /// </summary>
    public class FilterRequestDto
    {
        /// <summary>
        /// Tên thuộc tính cần lọc.
        /// Ví dụ: Name, CreationTime, IsEnabled,...
        /// </summary>
        public string Field { get; set; } = null!;

        /// <summary>
        /// Toán tử lọc.
        /// </summary>
        public CFilterOperator Operator { get; set; }

        /// <summary>
        /// Danh sách giá trị của điều kiện lọc.
        /// Tùy theo từng toán tử sẽ sử dụng số lượng phần tử khác nhau.
        /// </summary>
        public List<JsonElement> Values { get; set; } = [];

        /// <summary>
        /// Toán tử logic với điều kiện phía trước.
        /// </summary>
        public CLogicalOperator LogicalOperator { get; set; } = CLogicalOperator.And;
    }

    /// <summary>
    /// DTO đại diện cho một điều kiện sắp xếp.
    /// </summary>
    public class SortRequestDto
    {
        /// <summary>
        /// Tên thuộc tính cần sắp xếp.
        /// </summary>
        public string Field { get; set; } = null!;

        /// <summary>
        /// Hướng sắp xếp.
        /// </summary>
        public CSortDirection Direction { get; set; } = CSortDirection.Asc;
    }

    /// <summary>
    /// DTO chứa thông tin phân trang.
    /// </summary>
    public class PageInfoDto
    {
        /// <summary>
        /// Trang hiện tại.
        /// </summary>
        public int PageNumber { get; init; }

        /// <summary>
        /// Kích thước trang.
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// Tổng số bản ghi.
        /// </summary>
        public int TotalRecords { get; init; }

        /// <summary>
        /// Tổng số trang.
        /// </summary>
        public int TotalPages { get; init; }

        /// <summary>
        /// Có trang trước hay không.
        /// </summary>
        public bool HasPreviousPage { get; init; }

        /// <summary>
        /// Có trang tiếp theo hay không.
        /// </summary>
        public bool HasNextPage { get; init; }
    }

    /// <summary>
    /// DTO đại diện cho kết quả phân trang.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu.</typeparam>
    public class PagedResponseDto<T>
    {
        /// <summary>
        /// Danh sách dữ liệu.
        /// </summary>
        public IReadOnlyCollection<T> Items { get; init; } = [];

        /// <summary>
        /// Thông tin phân trang.
        /// </summary>
        public required PageInfoDto PageInfo { get; init; }
    }

    /// <summary>
    /// Hướng sắp xếp.
    /// </summary>
    public enum CSortDirection
    {
        /// <summary>
        /// Tăng dần.
        /// </summary>
        Asc = 0,

        /// <summary>
        /// Giảm dần.
        /// </summary>
        Desc = 1
    }

    /// <summary>
    /// Toán tử lọc.
    /// </summary>
    public enum CFilterOperator
    {
        /// <summary>
        /// Bằng.
        /// </summary>
        Equal = 0,

        /// <summary>
        /// Khác.
        /// </summary>
        NotEqual = 1,

        /// <summary>
        /// Lớn hơn.
        /// </summary>
        GreaterThan = 2,

        /// <summary>
        /// Lớn hơn hoặc bằng.
        /// </summary>
        GreaterThanOrEqual = 3,

        /// <summary>
        /// Nhỏ hơn.
        /// </summary>
        LessThan = 4,

        /// <summary>
        /// Nhỏ hơn hoặc bằng.
        /// </summary>
        LessThanOrEqual = 5,

        /// <summary>
        /// Chứa chuỗi.
        /// </summary>
        Contains = 6,

        /// <summary>
        /// Bắt đầu bằng.
        /// </summary>
        StartsWith = 7,

        /// <summary>
        /// Kết thúc bằng.
        /// </summary>
        EndsWith = 8,

        /// <summary>
        /// Thuộc tập hợp.
        /// </summary>
        In = 9,

        /// <summary>
        /// Không thuộc tập hợp.
        /// </summary>
        NotIn = 10,

        /// <summary>
        /// Nằm trong khoảng.
        /// </summary>
        Between = 11,

        /// <summary>
        /// Giá trị null.
        /// </summary>
        IsNull = 12,

        /// <summary>
        /// Giá trị khác null.
        /// </summary>
        IsNotNull = 13
    }

    /// <summary>
    /// Toán tử logic giữa các điều kiện lọc.
    /// </summary>
    public enum CLogicalOperator
    {
        /// <summary>
        /// Và.
        /// </summary>
        And = 0,

        /// <summary>
        /// Hoặc.
        /// </summary>
        Or = 1
    }
}