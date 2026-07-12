using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.AIs
{
    /// <summary>
    /// AIConfigurationService là một dịch vụ quản lý cấu hình kết nối đến các nhà cung cấp AI.
    /// </summary>
    public class AIConfigurationService : IAIConfigurationService
    {
        private readonly ILogger<AIConfigurationService> _logger;
        private readonly ThumbnailDbContext _dbContext;
        private readonly AIConfigurationMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        public AIConfigurationService(
            ILogger<AIConfigurationService> logger,
            ThumbnailDbContext dbContext,
            AIConfigurationMapper mapper,
            IServiceProvider serviceProvider
        )
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Lấy danh sách các cấu hình AI với phân trang dựa trên yêu cầu được cung cấp.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="deletedOnly"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedResponseDto<AIConfigurationDetailDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<AIConfigurationEntity> query = _dbContext.AIConfigurations
                .AsNoTracking()
                .Include(x => x.AIProvider);

            if (includeDeleted.HasValue)
            {
                query = query.IgnoreQueryFilters();

                if (deletedOnly == true)
                {
                    query = query.Where(x => x.IsDeleted);
                }
                else if (includeDeleted == false)
                {
                    query = query.Where(x => !x.IsDeleted);
                }
            }

            query = query.ApplyQuery(request);

            return await query.ToPagedResponseAsync<AIConfigurationEntity, AIConfigurationDetailDto>(
                request,
                selector: x => _mapper.ToDetailDto(x),
                cancellationToken);
        }

        /// <summary>
        /// Lấy thông tin cấu hình AI theo Id. Nếu không tìm thấy, trả về null.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asNoTracking"></param>
        /// <param name="includeDetails"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AIConfigurationEntity?> GetByIdAsync(
            Guid id,
            bool asNoTracking = true,
            bool includeDetails = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<AIConfigurationEntity> query = _dbContext.AIConfigurations;

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (includeDetails)
            {
                query = query.Include(x => x.AIProvider);
            }

            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        /// <summary>
        /// Tạo một cấu hình AI mới dựa trên yêu cầu được cung cấp.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessException"></exception>
        public async Task<AIConfigurationDetailDto> CreateAsync(
            AIConfiurationCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var provider = await _dbContext.AIProviders
                .FirstOrDefaultAsync(x => x.Id == request.AIProviderId, cancellationToken);

            if (provider is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI provider với Id '{request.AIProviderId}'.",
                    code: "AI_PROVIDER_NOT_FOUND");
            }

            var duplicatedConfiguration = await _dbContext.AIConfigurations
                .AsNoTracking()
                .AnyAsync(x =>
                    x.AIProviderId == request.AIProviderId &&
                    x.Name == request.Name,
                    cancellationToken);

            if (duplicatedConfiguration)
            {
                throw new BusinessException(
                    message: $"Configuration '{request.Name}' đã tồn tại trong AI provider '{provider.Name}'.",
                    code: "AI_CONFIGURATION_ALREADY_EXISTS");
            }

            // Validate API key with the provider's engine
            await ValidateApiKeyWithEngineAsync(provider.ProviderType, request.ApiKey, cancellationToken);

            if (request.IsDefault)
            {
                var defaultConfigurations = await _dbContext.AIConfigurations
                    .Where(x =>
                        x.AIProviderId == request.AIProviderId
                        && x.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var configuration in defaultConfigurations)
                {
                    configuration.IsDefault = false;
                }
            }
            else
            {
                var hasAnyConfiguration = await _dbContext.AIConfigurations
                    .AnyAsync(x => x.AIProviderId == request.AIProviderId, cancellationToken);

                if (!hasAnyConfiguration)
                {
                    request.IsDefault = true;
                }
            }

            var entity = _mapper.ToEntity(request);

            await _dbContext.AIConfigurations.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            entity.AIProvider = provider;

            return _mapper.ToDetailDto(entity);
        }

        /// <summary>
        /// Cập nhật một cấu hình AI dựa trên ID và yêu cầu được cung cấp. Nếu cấu hình là mặc định, không thể bỏ mặc định mà không đặt một cấu hình khác làm mặc định.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessException"></exception>
        public async Task<AIConfigurationDetailDto> UpdateAsync(
            Guid id,
            AIConfigurationUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            var existingConfiguration = await _dbContext.AIConfigurations
                .Include(x => x.AIProvider)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (existingConfiguration is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI configuration với Id '{id}'.",
                    code: "AI_CONFIGURATION_NOT_FOUND");
            }

            if (existingConfiguration.IsDefault && !request.IsDefault)
            {
                throw new BusinessException(
                    message: "Không thể bỏ cấu hình mặc định. Vui lòng đặt một cấu hình khác làm mặc định trước.",
                    code: "AI_CONFIGURATION_DEFAULT_REQUIRED");
            }

            var provider = await _dbContext.AIProviders
                .FirstOrDefaultAsync(x => x.Id == request.AIProviderId, cancellationToken);

            if (provider is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI provider với Id '{request.AIProviderId}'.",
                    code: "AI_PROVIDER_NOT_FOUND");
            }

            var duplicatedConfiguration = await _dbContext.AIConfigurations
                .AsNoTracking()
                .AnyAsync(x =>
                    x.AIProviderId == request.AIProviderId &&
                    x.Name == request.Name &&
                    x.Id != id,
                    cancellationToken);

            if (duplicatedConfiguration)
            {
                throw new BusinessException(
                    message: $"Configuration '{request.Name}' đã tồn tại trong AI provider '{provider.Name}'.",
                    code: "AI_CONFIGURATION_ALREADY_EXISTS");
            }

            // Validate API key with the provider's engine (chỉ khi gửi key mới)
            if (!string.IsNullOrWhiteSpace(request.ApiKey))
            {
                await ValidateApiKeyWithEngineAsync(provider.ProviderType, request.ApiKey, cancellationToken);
            }

            if (request.IsDefault)
            {
                var defaultConfigurations = await _dbContext.AIConfigurations
                    .Where(x =>
                        x.AIProviderId == request.AIProviderId &&
                        x.IsDefault &&
                        x.Id != id)
                    .ToListAsync(cancellationToken);

                foreach (var configuration in defaultConfigurations)
                {
                    configuration.IsDefault = false;
                }
            }

            _mapper.ToEntity(request, existingConfiguration);

            existingConfiguration.AIProvider = provider;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.ToDetailDto(existingConfiguration);
        }

        /// <summary>
        /// Dặt một cấu hình AI làm mặc định dựa trên ID. Nếu cấu hình đã là mặc định, ném ra BusinessException.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessException"></exception>
        public async Task<AIConfigurationDetailDto> SetDefaultAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var configuration = await _dbContext.AIConfigurations
                .Include(x => x.AIProvider)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (configuration is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI configuration với Id '{id}'.",
                    code: "AI_CONFIGURATION_NOT_FOUND");
            }

            if (configuration.IsDefault)
            {
                throw new BusinessException(
                    message: $"AI configuration '{configuration.Name}' hiện đã là cấu hình mặc định.",
                    code: "AI_CONFIGURATION_ALREADY_DEFAULT");
            }

            var currentDefaultConfiguration = await _dbContext.AIConfigurations
                .Where(x =>
                    x.AIProviderId == configuration.AIProviderId &&
                    x.IsDefault &&
                    x.Id != configuration.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentDefaultConfiguration is not null)
            {
                currentDefaultConfiguration.IsDefault = false;
            }

            configuration.IsDefault = true;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.ToDetailDto(configuration);
        }

        /// <summary>
        /// Xóa (soft delete) một cấu hình AI dựa trên ID. Nếu cấu hình là mặc định, ném ra BusinessException.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessException"></exception>
        public async Task<AIConfigurationDetailDto> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var existingConfiguration = await _dbContext.AIConfigurations
                .Include(x => x.AIProvider)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (existingConfiguration is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI configuration với Id '{id}'.",
                    code: "AI_CONFIGURATION_NOT_FOUND");
            }

            if (existingConfiguration.IsDefault)
            {
                throw new BusinessException(
                    message: "Không thể xóa cấu hình mặc định. Vui lòng đặt một cấu hình khác làm mặc định trước.",
                    code: "AI_CONFIGURATION_DEFAULT_DELETE_NOT_ALLOWED");
            }

            _dbContext.AIConfigurations.Remove(existingConfiguration);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.ToDetailDto(existingConfiguration);
        }

        /// <summary>
        /// Kiểm tra API key có hợp lệ với engine của provider không.
        /// </summary>
        private async Task ValidateApiKeyWithEngineAsync(
            Enums.CAIProviderType providerType,
            string plainApiKey,
            CancellationToken cancellationToken)
        {
            // Tìm engine phù hợp với provider type
            var engine = _serviceProvider.GetServices<IAIEngine>()
                .FirstOrDefault(e => e.ProviderType == providerType);

            if (engine is null)
            {
                _logger.LogWarning("Không tìm thấy engine cho provider {ProviderType}, bỏ qua validate key.", providerType);
                return;
            }

            var isValid = await engine.ValidateApiKeyAsync(plainApiKey);

            if (!isValid)
            {
                throw new BusinessException(
                    message: $"API Key không hợp lệ cho provider. Vui lòng kiểm tra lại.",
                    code: "AI_CONFIGURATION_INVALID_API_KEY");
            }

            _logger.LogInformation("API key validated successfully for {ProviderType}.", providerType);
        }
    }
}