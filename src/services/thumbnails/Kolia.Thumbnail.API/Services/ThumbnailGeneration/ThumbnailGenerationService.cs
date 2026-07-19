using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.ThumbnailGeneration;
using Kolia.Thumbnail.API.Engines.AI;
using Kolia.Thumbnail.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.ThumbnailGeneration
{
    public class ThumbnailGenerationService : IThumbnailGenerationService
    {
        private readonly ThumbnailDbContext _db;
        private readonly IThumbnailImageGenerationEngine _imageGenEngine;

        public ThumbnailGenerationService(
            ThumbnailDbContext db,
            IThumbnailImageGenerationEngine imageGenEngine)
        {
            _db = db;
            _imageGenEngine = imageGenEngine;
        }

        public async Task<string> BuildPromptAsync(
            Guid projectId,
            IEnumerable<Guid> displayTextOptionIds,
            IEnumerable<Guid> referenceLibraryItemIds,
            Guid? characterId,
            string changesRequestText,
            string ratio,
            string resolution,
            CancellationToken ct = default)
        {
            var selectedTexts = await _db.DisplayTextOptions
                .Where(o => displayTextOptionIds.Contains(o.Id))
                .Select(o => o.Content)
                .ToListAsync(ct);

            string characterName = string.Empty;
            string characterLockNote = string.Empty;
            if (characterId.HasValue)
            {
                var charEntity = await _db.Characters
                    .Include(c => c.Images)
                    .FirstOrDefaultAsync(c => c.Id == characterId.Value, ct);
                characterName = charEntity?.Name ?? string.Empty;
                characterLockNote = charEntity != null
                    ? $"KHÓA — không đổi nhận diện nhân vật '{charEntity.Name}' (dựa trên {charEntity.Images.Count} ảnh tham khảo đã lưu)."
                    : string.Empty;
            }

            var referenceAnalyses = await _db.ThumbnailAnalyses
                .Where(a => referenceLibraryItemIds.Contains(a.ThumbnailLibraryItemId))
                .ToListAsync(ct);
            var lockedFactorsNote = string.Join(" ", referenceAnalyses.Select(a => a.ThumbnailFactorsJson));

            var prompt =
                $"[KHÓA — GIỮ NGUYÊN]: {characterLockNote} Bố cục và phong cách chính tham khảo từ: {lockedFactorsNote}\n" +
                $"[ĐƯỢC PHÉP THAY ĐỔI]: {changesRequestText}\n" +
                $"[CHỮ HIỂN THỊ TRÊN ẢNH]: {string.Join(", ", selectedTexts)}\n" +
                $"[KỸ THUẬT]: tỉ lệ {ratio}, độ phân giải {resolution}.";

            return prompt;
        }

        public async Task<GeneratedThumbnailSetEntity> GenerateAsync(
            Guid projectId,
            IEnumerable<Guid> displayTextOptionIds,
            IEnumerable<Guid> referenceLibraryItemIds,
            Guid? characterId,
            string changesRequestText,
            string ratio,
            string resolution,
            int requestedCount,
            string? overridePromptText = null,
            CancellationToken ct = default)
        {
            // Build prompt — nếu có overridePromptText thì dùng thẳng, không build lại
            var basePromptText = overridePromptText ?? await BuildPromptAsync(
                projectId, displayTextOptionIds, referenceLibraryItemIds,
                characterId, changesRequestText, ratio, resolution, ct);

            // Lấy character name cho snapshot
            var characterName = string.Empty;
            if (characterId.HasValue)
            {
                var charEntity = await _db.Characters.FindAsync([characterId.Value], ct);
                characterName = charEntity?.Name ?? string.Empty;
            }

            // Gọi Engine AI sinh ảnh
            var genResult = await _imageGenEngine.GenerateAsync(basePromptText, ratio, resolution, requestedCount, ct);

            // Đếm set index hiện tại
            var setIndex = await _db.GeneratedThumbnailSets
                .Where(s => s.ThumbnailGenerationRequest.ProjectId == projectId)
                .CountAsync(ct) + 1;

            var request = new ThumbnailGenerationRequestEntity
            {
                ProjectId = projectId,
                CharacterId = characterId,
                ChangesRequestText = changesRequestText,
                GeneratedPromptText = basePromptText,
                Ratio = ratio,
                Resolution = resolution,
                RequestedImageCount = requestedCount
            };

            foreach (var optId in displayTextOptionIds)
            {
                request.SelectedDisplayTextOptions.Add(new ThumbnailGenerationRequestDisplayTextEntity
                {
                    ThumbnailGenerationRequestId = request.Id,
                    DisplayTextOptionId = optId
                });
            }

            foreach (var refId in referenceLibraryItemIds)
            {
                request.SelectedReferenceItems.Add(new ThumbnailGenerationRequestReferenceEntity
                {
                    ThumbnailGenerationRequestId = request.Id,
                    ThumbnailLibraryItemId = refId
                });
            }

            _db.ThumbnailGenerationRequests.Add(request);

            // Lấy lại selectedTexts cho DisplayTextSnapshot (cần thiết khi dùng overridePromptText)
            var selectedTexts = await _db.DisplayTextOptions
                .Where(o => displayTextOptionIds.Contains(o.Id))
                .Select(o => o.Content)
                .ToListAsync(ct);

            var set = new GeneratedThumbnailSetEntity
            {
                ThumbnailGenerationRequestId = request.Id,
                SetIndex = setIndex
            };

            for (int i = 0; i < genResult.ImageUrls.Count; i++)
            {
                set.Variants.Add(new GeneratedThumbnailEntity
                {
                    GeneratedThumbnailSetId = set.Id,
                    VariantIndex = i + 1,
                    VersionNumber = 1,
                    ImageUrl = genResult.ImageUrls[i],
                    DisplayTextSnapshot = string.Join(", ", selectedTexts),
                    CharacterSnapshotName = characterName,
                    IsApproved = false,
                    WasDownloaded = false,
                    IsPushedToTitleStep = false
                });
            }

            _db.GeneratedThumbnailSets.Add(set);
            await _db.SaveChangesAsync(ct);

            return set;
        }

        public async Task<GeneratedThumbnailEntity> EditAsync(
            Guid generatedThumbnailId,
            CThumbnailEditTool editTool,
            string editRequestText,
            Guid? secondaryReferenceLibraryItemId = null,
            Guid? secondaryCharacterImageId = null,
            CancellationToken ct = default)
        {
            var original = await _db.GeneratedThumbnails
                .FirstOrDefaultAsync(t => t.Id == generatedThumbnailId, ct)
                ?? throw new KeyNotFoundException($"GeneratedThumbnail {generatedThumbnailId} không tồn tại.");

            // Xác định ảnh phụ theo từng loại tool
            string? secondaryImageUrl = editTool switch
            {
                CThumbnailEditTool.Style when secondaryReferenceLibraryItemId.HasValue =>
                    (await _db.ThumbnailLibraryItems.FindAsync([secondaryReferenceLibraryItemId.Value], ct))?.ThumbnailImageUrl,
                CThumbnailEditTool.Avatar when secondaryCharacterImageId.HasValue =>
                    (await _db.CharacterImages.FindAsync([secondaryCharacterImageId.Value], ct))?.ImageUrl,
                _ => null
            };

            // Gọi AI chỉnh sửa
            var editedImageUrl = await _imageGenEngine.EditAsync(original.ImageUrl, editRequestText, secondaryImageUrl, ct);

            // Xác định phiên bản tiếp theo
            var nextVersionNumber = original.VersionNumber + 1;

            var editedThumbnail = new GeneratedThumbnailEntity
            {
                GeneratedThumbnailSetId = original.GeneratedThumbnailSetId,
                VariantIndex = original.VariantIndex,
                ParentGeneratedThumbnailId = original.Id,
                VersionNumber = nextVersionNumber,
                ImageUrl = editedImageUrl,
                DisplayTextSnapshot = original.DisplayTextSnapshot,
                CharacterSnapshotName = original.CharacterSnapshotName,
                LastEditTool = editTool,
                LastEditRequestText = editRequestText,
                IsApproved = false,
                WasDownloaded = false,
                IsPushedToTitleStep = false
            };

            _db.GeneratedThumbnails.Add(editedThumbnail);
            await _db.SaveChangesAsync(ct);

            return editedThumbnail;
        }

        public async Task<IReadOnlyList<GeneratedThumbnailSetEntity>> GetSetsAsync(
            Guid projectId, CancellationToken ct = default)
        {
            return await _db.GeneratedThumbnailSets
                .Include(s => s.Variants)
                .Where(s => s.ThumbnailGenerationRequest.ProjectId == projectId)
                .OrderByDescending(s => s.SetIndex)
                .ToListAsync(ct);
        }

        public async Task ApproveAsync(Guid generatedThumbnailId, CancellationToken ct = default)
        {
            var item = await _db.GeneratedThumbnails.FindAsync([generatedThumbnailId], ct)
                ?? throw new KeyNotFoundException($"GeneratedThumbnail {generatedThumbnailId} không tồn tại.");

            item.IsApproved = true;
            item.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task PushToTitleStepAsync(Guid generatedThumbnailId, CancellationToken ct = default)
        {
            var item = await _db.GeneratedThumbnails.FindAsync([generatedThumbnailId], ct)
                ?? throw new KeyNotFoundException($"GeneratedThumbnail {generatedThumbnailId} không tồn tại.");

            item.IsPushedToTitleStep = true;
            item.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkDownloadedAsync(Guid generatedThumbnailId, CancellationToken ct = default)
        {
            var item = await _db.GeneratedThumbnails.FindAsync([generatedThumbnailId], ct)
                ?? throw new KeyNotFoundException($"GeneratedThumbnail {generatedThumbnailId} không tồn tại.");

            item.WasDownloaded = true;
            item.LastModificationTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
