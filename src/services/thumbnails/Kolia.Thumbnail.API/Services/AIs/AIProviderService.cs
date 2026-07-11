using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.AIs;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.AIs
{
    public class AIProviderService : IAIProviderService
    {
        private readonly ThumbnailDbContext _dbContext;
        private readonly ILogger<AIProviderService> _logger;
        public AIProviderService(
            ThumbnailDbContext dbContext,
            ILogger<AIProviderService> logger
        )
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các nhà cung cấp AI với phân trang dựa trên yêu cầu được cung cấp.
        /// Trả về một đối tượng PagedResponseDto chứa danh sách các nhà cung cấp AI và thông tin phân trang.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedResponseDto<AIProviderDetailDto>> GetWithPagingAsync(
            PagedRequestDto request,
            CancellationToken cancellationToken = default)
        {
            IQueryable<AIProviderEntity> query = _dbContext.AIProviders
                .AsNoTracking();

            query = query.ApplyQuery(request);

            return await query.ToPagedResponseAsync<AIProviderEntity, AIProviderDetailDto>(
                request,
                selector: p => p.ToDetailDto(),
                cancellationToken);
        }

        /// <summary>
        /// Tạo một nhà cung cấp AI mới trong cơ sở dữ liệu.
        /// Nếu quá trình tạo thành công, trả về thực thể nhà cung cấp AI đã được tạo;
        /// nếu có lỗi xảy ra, ném ra BusinessException với thông tin chi tiết về lỗi.
        /// </summary>
        /// <param name="aIProviderCreateDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<AIProviderEntity?> CreateAsync(AIProviderCreateDto aIProviderCreateDto,
            CancellationToken cancellationToken = default)
        {
            var existingProvider = await _dbContext.AIProviders.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == aIProviderCreateDto.Name, cancellationToken);

            if (existingProvider != null)
            {
                throw new BusinessException(message: $"AI provider có tên '{aIProviderCreateDto.Name}' đã tồn tại.", code: "AI_PROVIDER_ALREADY_EXISTS");
            }

            AIProviderEntity aiProviderEntity = aIProviderCreateDto.ToEntity();
            await _dbContext.AIProviders.AddAsync(aiProviderEntity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return aiProviderEntity;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhà cung cấp AI dựa trên ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asNoTracking"></param>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AIProviderEntity?> GetByIdAsync(Guid id,
            bool asNoTracking = true,
            bool includeDetails = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<AIProviderEntity> query = _dbContext.AIProviders;

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (includeDetails)
            {
                query = query.Include(p => p.Configurations);
            }

            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
    }
}