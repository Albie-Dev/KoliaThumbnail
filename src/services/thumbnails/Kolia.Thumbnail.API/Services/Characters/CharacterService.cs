using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.Characters;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.Characters
{
    public class CharacterService : ICharacterService
    {
        private readonly ThumbnailDbContext _db;

        public CharacterService(ThumbnailDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CharacterEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Characters
                .Include(c => c.Images)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync(ct);
        }

        public async Task<CharacterEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Characters
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public async Task<CharacterEntity> CreateAsync(
            string name, string? description, CancellationToken ct = default)
        {
            var character = new CharacterEntity
            {
                Name = name,
                Description = description
            };

            _db.Characters.Add(character);
            await _db.SaveChangesAsync(ct);
            return character;
        }

        public async Task<CharacterImageEntity> AddImageAsync(
            Guid characterId,
            string imageUrl,
            string? expressionLabel,
            string? angleLabel,
            bool isPrimary,
            CancellationToken ct = default)
        {
            var character = await _db.Characters
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == characterId && !c.IsDeleted, ct)
                ?? throw new KeyNotFoundException($"Character {characterId} không tồn tại.");

            // Nếu ảnh mới là primary, bỏ primary các ảnh cũ
            if (isPrimary)
            {
                foreach (var img in character.Images)
                {
                    img.IsPrimary = false;
                }
            }

            var newImage = new CharacterImageEntity
            {
                CharacterId = characterId,
                ImageUrl = imageUrl,
                ExpressionLabel = expressionLabel,
                AngleLabel = angleLabel,
                IsPrimary = isPrimary
            };

            _db.CharacterImages.Add(newImage);
            await _db.SaveChangesAsync(ct);
            return newImage;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var character = await _db.Characters.FindAsync([id], ct)
                ?? throw new KeyNotFoundException($"Character {id} không tồn tại.");

            character.IsDeleted = true;
            character.DeletionTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
