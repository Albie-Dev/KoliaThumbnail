using Kolia.Thumbnail.API.Attributes;

namespace Kolia.Thumbnail.API.Data.Entities
{
    /// <summary>
    /// Entity cơ sở cho tất cả các entity trong hệ thống, cung cấp các thuộc tính chung như Id, CreationTime, LastModificationTime, IsDeleted và DeletionTime
    /// </summary>
    public abstract class BaseEntity : ISoftDelete
    {
        /// <summary>
        /// Id của entity, được sử dụng để định danh duy nhất cho mỗi entity
        /// </summary>
        public Guid Id { get; private set; } = Guid.CreateVersion7(DateTimeOffset.UtcNow);
        /// <summary>
        /// Thời gian tạo của entity, được sử dụng để theo dõi khi nào entity được tạo ra
        /// </summary>
        [Queryable(
            Filterable = true,
            RangeFilterable = true,
            Sortable = true
        )]
        public DateTimeOffset CreationTime { get; private set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// Thời gian sửa đổi cuối cùng của entity, được sử dụng để theo dõi khi nào entity được sửa đổi lần cuối
        /// </summary>
        [Queryable(
            Filterable = true,
            RangeFilterable = true,
            Sortable = true
        )]
        public DateTimeOffset? LastModificationTime { get; set; } = null;
        /// <summary>
        /// Trạng thái xóa của entity, được sử dụng để đánh dấu entity đã bị xóa mà không thực sự xóa nó khỏi cơ sở dữ liệu
        /// </summary>
        [Queryable(
            Filterable = true
        )]
        public bool IsDeleted { get; set; } = false;
        /// <summary>
        /// Thời gian xóa của entity, được sử dụng để theo dõi khi nào entity bị xóa
        /// </summary>
        [Queryable(
            Filterable = true,
            RangeFilterable = true,
            Sortable = true
        )]
        public DateTimeOffset? DeletionTime { get; set; } = null;
    }
}