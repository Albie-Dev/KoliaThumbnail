using Kolia.Thumbnail.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kolia.Thumbnail.API.Data.Interceptors
{
    /// <summary>
    /// Interceptor được sử dụng để tự động cập nhật các thuộc tính audit (CreationTime, LastModificationTime, IsDeleted, DeletionTime) của các entity kế thừa từ BaseEntity khi thực hiện các thao tác thêm, sửa hoặc xóa entity trong DbContext.
    /// </summary>
    public sealed class AuditEntityInterceptor : SaveChangesInterceptor
    {
        /// <summary>
        /// Ghi đè phương thức SavingChanges để cập nhật các thuộc tính audit của các entity trước khi lưu thay đổi vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        /// <summary>
        /// Ghi đè phương thức SavingChangesAsync để cập nhật các thuộc tính audit của các entity trước khi lưu thay đổi vào cơ sở dữ liệu một cách bất đồng bộ.
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Cập nhật các thuộc tính audit của các entity kế thừa từ BaseEntity dựa trên trạng thái của chúng trong ChangeTracker của DbContext.
        /// </summary>
        /// <param name="context"></param>
        private static void UpdateEntities(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;

            foreach (EntityEntry<BaseEntity> entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(x => x.CreationTime).CurrentValue = now;
                        entry.Property(x => x.LastModificationTime).CurrentValue = null;
                        entry.Property(x => x.IsDeleted).CurrentValue = false;
                        entry.Property(x => x.DeletionTime).CurrentValue = null;
                        break;

                    case EntityState.Modified:
                        entry.Property(x => x.LastModificationTime).CurrentValue = now;
                        entry.Property(x => x.CreationTime).IsModified = false;
                        entry.Property(x => x.IsDeleted).IsModified = false;
                        entry.Property(x => x.DeletionTime).IsModified = false;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Property(x => x.IsDeleted).CurrentValue = true;
                        entry.Property(x => x.DeletionTime).CurrentValue = now;
                        entry.Property(x => x.LastModificationTime).CurrentValue = now;
                        entry.Property(x => x.CreationTime).IsModified = false;
                        break;
                }
            }
        }
    }
}