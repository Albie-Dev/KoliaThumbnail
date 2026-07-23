namespace Kolia.Thumbnail.API.Enums
{
    /// <summary>
    /// Kiểu phân trang cho REST API fetch.
    /// </summary>
    public enum CApiPaginationType
    {
        /// <summary>Không phân trang — chỉ fetch 1 lần.</summary>
        None = 0,

        /// <summary>Phân trang dạng offset (param: offset, limit).</summary>
        Offset = 1,

        /// <summary>Phân trang dạng page (param: page, pageSize).</summary>
        Page = 2,

        /// <summary>Phân trang dạng cursor (param: cursor, lấy từ response).</summary>
        Cursor = 3
    }
}
