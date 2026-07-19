using Kolia.Thumbnail.API.Data.Entities.Characters;

namespace Kolia.Thumbnail.API.Services.Characters
{
    /// <summary>
    /// Quản lý nhân vật và ảnh biểu cảm toàn cục.
    /// </summary>
    public interface ICharacterService
    {
        /// <summary>
        /// Lấy tất cả nhân vật kèm ảnh primary.
        /// </summary>
        Task<IReadOnlyList<CharacterEntity>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Lấy chi tiết 1 nhân vật kèm tất cả ảnh.
        /// </summary>
        Task<CharacterEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Tạo nhân vật mới.
        /// </summary>
        Task<CharacterEntity> CreateAsync(string name, string? description,
            CancellationToken ct = default);

        /// <summary>
        /// Thêm ảnh mới cho nhân vật.
        /// </summary>
        Task<CharacterImageEntity> AddImageAsync(Guid characterId, string imageUrl,
            string? expressionLabel, string? angleLabel, bool isPrimary,
            CancellationToken ct = default);

        /// <summary>
        /// Xóa mềm nhân vật.
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
