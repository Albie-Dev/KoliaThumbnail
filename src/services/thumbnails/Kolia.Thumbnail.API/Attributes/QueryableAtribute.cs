namespace Kolia.Thumbnail.API.Attributes
{
    /// <summary>
    /// Đánh dấu một thuộc tính được phép tham gia vào các thao tác truy vấn động như
    /// tìm kiếm, lọc, sắp xếp, nhóm dữ liệu hoặc xuất dữ liệu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class QueryableAttribute : Attribute
    {
        /// <summary>
        /// Cho phép tìm kiếm trên thuộc tính.
        /// </summary>
        public bool Searchable { get; init; } = false;

        /// <summary>
        /// Cho phép lọc theo thuộc tính.
        /// </summary>
        public bool Filterable { get; init; } = false;

        /// <summary>
        /// Cho phép lọc theo khoảng giá trị (From/To) với toán tử <c>&gt;=</c> và <c>&lt;=</c>.
        /// Chỉ có hiệu lực với các kiểu số và ngày tháng hỗ trợ toán tử so sánh
        /// (ví dụ: <c>DateTime</c>, <c>int</c>, <c>long</c>, <c>decimal</c>,...).
        /// Kiểu không hỗ trợ so sánh (như <c>string</c>, <c>Guid</c>) sẽ bị bỏ qua dù flag này là <c>true</c>.
        /// </summary>
        public bool RangeFilterable { get; init; } = false;

        /// <summary>
        /// Cho phép sắp xếp theo thuộc tính.
        /// </summary>
        public bool Sortable { get; init; } = false;

        /// <summary>
        /// Cho phép nhóm dữ liệu theo thuộc tính.
        /// Dùng cho báo cáo hoặc thống kê.
        /// </summary>
        public bool Groupable { get; init; } = false;

        /// <summary>
        /// Cho phép xuất thuộc tính khi export dữ liệu.
        /// </summary>
        public bool Exportable { get; init; } = false;

        /// <summary>
        /// Tên hiển thị của thuộc tính.
        /// Nếu không thiết lập sẽ sử dụng tên property.
        /// </summary>
        public string? DisplayName { get; init; }

        /// <summary>
        /// Thứ tự ưu tiên khi Search.
        /// Giá trị nhỏ hơn sẽ được ưu tiên trước.
        /// </summary>
        public int SearchOrder { get; init; }

        /// <summary>
        /// Không phân biệt chữ hoa/thường khi Search.
        /// </summary>
        public bool IgnoreCase { get; init; } = true;
    }
}