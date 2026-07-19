using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Models;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Security;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.AIs
{
    /// <summary>
    /// Service quản lý cấu hình AI cho từng chức năng nghiệp vụ.
    /// Mỗi function config xác định provider, config, model và danh sách fallback
    /// cho một chức năng cụ thể (ContentBriefAnalysis, NewsScoring, ...).
    /// </summary>
    public sealed class AIFunctionConfigService : IAIFunctionConfigService
    {
        private readonly ThumbnailDbContext _db;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApiKeyProtector _apiKeyProtector;

        public AIFunctionConfigService(
            ThumbnailDbContext db,
            IServiceProvider serviceProvider,
            IApiKeyProtector apiKeyProtector)
        {
            _db = db;
            _serviceProvider = serviceProvider;
            _apiKeyProtector = apiKeyProtector;
        }

        public async Task<PagedResponseDto<AIFunctionConfigSummaryDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            IQueryable<AIFunctionConfigEntity> query = _db.AIFunctionConfigs
                .AsNoTracking()
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.AIProvider)
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.AIProviderConfiguration);

            // Soft-delete handling
            if (includeDeleted.HasValue)
            {
                query = query.IgnoreQueryFilters();
                if (deletedOnly == true)
                    query = query.Where(x => x.IsDeleted);
                else if (includeDeleted == false)
                    query = query.Where(x => !x.IsDeleted);
            }

            query = query.ApplyQuery(request);

            return await query.ToPagedResponseAsync<AIFunctionConfigEntity, AIFunctionConfigSummaryDto>(
                request,
                selector: entity => new AIFunctionConfigSummaryDto
                {
                    Id = entity.Id,
                    FunctionType = entity.FunctionType,
                    Model = entity.Items
                        .Where(i => !i.IsDeleted && i.Priority == 0)
                        .Select(i => i.Model ?? entity.Model)
                        .FirstOrDefault(),
                    Temperature = entity.Items
                        .Where(i => !i.IsDeleted && i.Priority == 0)
                        .Select(i => i.Temperature ?? entity.Temperature)
                        .FirstOrDefault(),
                    MaxTokens = entity.Items
                        .Where(i => !i.IsDeleted && i.Priority == 0)
                        .Select(i => i.MaxTokens ?? entity.MaxTokens)
                        .FirstOrDefault(),
                    PrimaryProviderName = entity.Items
                        .Where(i => !i.IsDeleted && i.Priority == 0)
                        .Select(i => i.AIProvider.Name)
                        .FirstOrDefault(),
                    PrimaryConfigName = entity.Items
                        .Where(i => !i.IsDeleted && i.Priority == 0)
                        .Select(i => i.AIProviderConfiguration.Name)
                        .FirstOrDefault(),
                    FallbackCount = entity.Items
                        .Count(i => !i.IsDeleted && i.Priority > 0),
                    CreationTime = entity.CreationTime,
                    LastModificationTime = entity.LastModificationTime,
                },
                ct);
        }

        public async Task<AIFunctionConfigDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.AIFunctionConfigs
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.AIProvider)
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.AIProviderConfiguration)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy cấu hình chức năng AI với Id '{id}'.");

            return MapToDetailDto(entity);
        }

        public async Task<AIFunctionConfigDetailDto> GetByFunctionTypeAsync(CAIFunctionType functionType, CancellationToken ct = default)
        {
            var entity = await _db.AIFunctionConfigs
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.AIProvider)
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.AIProviderConfiguration)
                .FirstOrDefaultAsync(x => x.FunctionType == functionType && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy cấu hình cho chức năng '{functionType}'.");

            return MapToDetailDto(entity);
        }

        public async Task<AIFunctionConfigDetailDto> CreateAsync(CreateAIFunctionConfigDto dto, CancellationToken ct = default)
        {
            // Kiểm tra function type đã tồn tại chưa
            var exists = await _db.AIFunctionConfigs
                .AnyAsync(x => x.FunctionType == dto.FunctionType && !x.IsDeleted, ct);

            if (exists)
                throw new BusinessException($"Chức năng '{dto.FunctionType}' đã có cấu hình.", "FUNCTION_CONFIG_EXISTS");

            // Validate providers & configs tồn tại
            await ValidateItemsAsync(dto.Items.Select(i => (i.AIProviderId, i.AIProviderConfigurationId)), ct);

            var entity = new AIFunctionConfigEntity
            {
                FunctionType = dto.FunctionType,
                Model = dto.Model,
                Temperature = dto.Temperature,
                MaxTokens = dto.MaxTokens,
                Items = dto.Items.Select(i => new AIFunctionConfigItemEntity
                {
                    Priority = i.Priority,
                    AIProviderId = i.AIProviderId,
                    AIProviderConfigurationId = i.AIProviderConfigurationId,
                    Model = i.Model,
                    Temperature = i.Temperature,
                    MaxTokens = i.MaxTokens,
                    IsEnabled = true,
                }).ToList(),
            };

            _db.AIFunctionConfigs.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Load lại với includes để trả về DTO đầy đủ
            return await GetByIdAsync(entity.Id, ct);
        }

        public async Task<AIFunctionConfigDetailDto> UpdateAsync(Guid id, UpdateAIFunctionConfigDto dto, CancellationToken ct = default)
        {
            var entity = await _db.AIFunctionConfigs
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy cấu hình chức năng AI với Id '{id}'.");

            // Validate providers & configs tồn tại
            await ValidateItemsAsync(dto.Items.Select(i => (i.AIProviderId, i.AIProviderConfigurationId)), ct);

            entity.Model = dto.Model;
            entity.Temperature = dto.Temperature;
            entity.MaxTokens = dto.MaxTokens;
            entity.LastModificationTime = DateTimeOffset.UtcNow;

            // Xoá items cũ bằng bulk delete — tránh lỗi concurrency
            await _db.AIFunctionConfigItems
                .Where(i => i.FunctionConfigId == id)
                .ExecuteDeleteAsync(ct);

            // Thêm items mới
            foreach (var itemDto in dto.Items)
            {
                _db.AIFunctionConfigItems.Add(new AIFunctionConfigItemEntity
                {
                    FunctionConfigId = id,
                    Priority = itemDto.Priority,
                    AIProviderId = itemDto.AIProviderId,
                    AIProviderConfigurationId = itemDto.AIProviderConfigurationId,
                    Model = itemDto.Model,
                    Temperature = itemDto.Temperature,
                    MaxTokens = itemDto.MaxTokens,
                    IsEnabled = itemDto.IsEnabled,
                });
            }

            await _db.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.AIFunctionConfigs
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy cấu hình chức năng AI với Id '{id}'.");

            entity.IsDeleted = true;
            entity.DeletionTime = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<List<AIModelInfo>> GetProviderModelsAsync(
            Guid providerId, Guid configurationId,
            CancellationToken ct = default)
        {
            // 1. Lấy provider để biết ProviderType
            var provider = await _db.AIProviders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == providerId && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"AI Provider với Id '{providerId}' không tồn tại.");

            // 2. Lấy config để giải mã API key
            var config = await _db.AIProviderConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == configurationId && !x.IsDeleted && x.IsEnabled, ct)
                ?? throw new NotFoundException($"AI Configuration với Id '{configurationId}' không tồn tại hoặc không hoạt động.");

            // 3. Tìm engine phù hợp với provider type
            var engines = _serviceProvider.GetServices<IAIEngine>();
            var engine = engines.FirstOrDefault(e => e.ProviderType == provider.ProviderType)
                ?? throw new NotSupportedException($"Không tìm thấy engine cho provider '{provider.Name}'.");

            // 4. Giải mã API key và gọi API lấy danh sách models
            var apiKey = _apiKeyProtector.Unprotect(config.ApiKey);
            return await engine.GetAIModelInfosAsync(apiKey);
        }

        // ── Private helpers ──

        private async Task ValidateItemsAsync(
            IEnumerable<(Guid ProviderId, Guid ConfigId)> items,
            CancellationToken ct)
        {
            foreach (var (providerId, configId) in items)
            {
                var providerExists = await _db.AIProviders
                    .AnyAsync(x => x.Id == providerId && !x.IsDeleted, ct);
                if (!providerExists)
                    throw new NotFoundException($"AI Provider với Id '{providerId}' không tồn tại.");

                var configExists = await _db.AIProviderConfigurations
                    .AnyAsync(x => x.Id == configId && !x.IsDeleted, ct);
                if (!configExists)
                    throw new NotFoundException($"AI Configuration với Id '{configId}' không tồn tại.");
            }
        }

        private static AIFunctionConfigDetailDto MapToDetailDto(AIFunctionConfigEntity entity)
        {
            var items = entity.Items?
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.Priority)
                .Select(i => new AIFunctionConfigItemDetailDto
                {
                    Id = i.Id,
                    Priority = i.Priority,
                    AIProviderId = i.AIProviderId,
                    AIProviderName = i.AIProvider?.Name ?? "N/A",
                    AIProviderConfigurationId = i.AIProviderConfigurationId,
                    AIProviderConfigurationName = i.AIProviderConfiguration?.Name ?? "N/A",
                    Model = i.Model,
                    Temperature = i.Temperature,
                    MaxTokens = i.MaxTokens,
                    IsEnabled = i.IsEnabled,
                })
                .ToList() ?? new();

            var primaryItem = items.FirstOrDefault(i => i.Priority == 0);

            return new AIFunctionConfigDetailDto
            {
                Id = entity.Id,
                FunctionType = entity.FunctionType,
                Model = primaryItem?.Model ?? entity.Model,
                Temperature = primaryItem?.Temperature ?? entity.Temperature,
                MaxTokens = primaryItem?.MaxTokens ?? entity.MaxTokens,
                CreationTime = entity.CreationTime,
                LastModificationTime = entity.LastModificationTime,
                Items = items,
            };
        }
    }
}
