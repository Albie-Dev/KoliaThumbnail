using System.Linq.Expressions;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kolia.Thumbnail.API.Data.Extensions
{
    /// <summary>
    /// Các phương thức mở rộng cho ModelBuilder để áp dụng các bộ lọc truy vấn mềm xóa (soft delete) cho các entity triển khai ISoftDelete
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Áp dụng bộ lọc truy vấn mềm xóa (soft delete) cho tất cả các entity triển khai ISoftDelete trong ModelBuilder. Bộ lọc này sẽ tự động loại bỏ các entity đã bị đánh dấu là xóa (IsDeleted = true) khỏi kết quả truy vấn.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                var parameter = Expression.Parameter(entityType.ClrType, "e");

                var property = Expression.Property(
                    parameter,
                    nameof(ISoftDelete.IsDeleted));

                var compare = Expression.Equal(
                    property,
                    Expression.Constant(false));

                var lambda = Expression.Lambda(compare, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }
    }
}