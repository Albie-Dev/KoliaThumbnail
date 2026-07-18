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
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<AIProviderEntity> query = _dbContext.AIProviders
                .AsNoTracking();

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
        public async Task<AIProviderDetailDto> CreateAsync(AIProviderCreateDto aIProviderCreateDto,
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
            return aiProviderEntity.ToDetailDto();
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

        /// <summary>
        /// Cập nhật thông tin của một nhà cung cấp AI dựa trên ID.
        /// Nếu nhà cung cấp AI không tồn tại, ném ra NotFoundException;
        /// nếu tên đã tồn tại bởi nhà cung cấp khác, ném ra BusinessException.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessException"></exception>
        public async Task<AIProviderDetailDto> UpdateAsync(Guid id,
            AIProviderUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra tồn tại
            var existingProvider = await _dbContext.AIProviders
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (existingProvider is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI provider với Id '{id}'.",
                    code: "AI_PROVIDER_NOT_FOUND");
            }

            // 2. Kiểm tra trùng tên (loại trừ chính nó)
            var duplicateName = await _dbContext.AIProviders.AsNoTracking()
                .AnyAsync(p => p.Name == request.Name && p.Id != id, cancellationToken);

            if (duplicateName)
            {
                throw new BusinessException(
                    message: $"AI provider có tên '{request.Name}' đã tồn tại.",
                    code: "AI_PROVIDER_ALREADY_EXISTS");
            }

            // 3. Map dữ liệu từ DTO sang entity
            request.ToEntity(existingProvider);

            // 4. Lưu thay đổi
            await _dbContext.SaveChangesAsync(cancellationToken);

            return existingProvider.ToDetailDto();
        }

        /// <summary>
        /// Xoá (soft delete) một nhà cung cấp AI dựa trên ID.
        /// Interceptor AuditEntityInterceptor tự động chuyển EntityState.Deleted
        /// thành Modified và set IsDeleted=true, DeletionTime, LastModificationTime.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<AIProviderDetailDto> DeleteAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            var existingProvider = await _dbContext.AIProviders
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (existingProvider is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy AI provider với Id '{id}'.",
                    code: "AI_PROVIDER_NOT_FOUND");
            }

            _dbContext.AIProviders.Remove(existingProvider);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return existingProvider.ToDetailDto();
        }
    }
}