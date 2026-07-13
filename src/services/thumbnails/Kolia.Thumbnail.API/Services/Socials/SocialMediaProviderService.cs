using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.Socials;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Models.SocialMedias;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Socials
{
    public class SocialMediaProviderService : ISocialMediaProviderService
    {
        private readonly ThumbnailDbContext _dbContext;
        private readonly ILogger<SocialMediaProviderService> _logger;
        public SocialMediaProviderService(
            ThumbnailDbContext dbContext,
            ILogger<SocialMediaProviderService> logger
        )
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các nhà cung cấp SOCIAL_MEDIA với phân trang dựa trên yêu cầu được cung cấp.
        /// Trả về một đối tượng PagedResponseDto chứa danh sách các nhà cung cấp SOCIAL_MEDIA và thông tin phân trang.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedResponseDto<SocialMediaProviderDetailDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<SocialMediaProviderEntity> query = _dbContext.SocialMediaProviders
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

            return await query.ToPagedResponseAsync<SocialMediaProviderEntity, SocialMediaProviderDetailDto>(
                request,
                selector: p => p.ToDetailDto(),
                cancellationToken);
        }

        /// <summary>
        /// Tạo một nhà cung cấp SOCIAL_MEDIA mới trong cơ sở dữ liệu.
        /// Nếu quá trình tạo thành công, trả về thực thể nhà cung cấp SOCIAL_MEDIA đã được tạo;
        /// nếu có lỗi xảy ra, ném ra BusinessException với thông tin chi tiết về lỗi.
        /// </summary>
        /// <param name="SocialMediaProviderCreateDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<SocialMediaProviderDetailDto> CreateAsync(SocialMediaProviderCreateDto SocialMediaProviderCreateDto,
            CancellationToken cancellationToken = default)
        {
            var existingProvider = await _dbContext.SocialMediaProviders.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == SocialMediaProviderCreateDto.Name, cancellationToken);

            if (existingProvider != null)
            {
                throw new BusinessException(message: $"SOCIAL_MEDIA provider có tên '{SocialMediaProviderCreateDto.Name}' đã tồn tại.", code: "SOCIAL_MEDIA_PROVIDER_ALREADY_EXISTS");
            }

            SocialMediaProviderEntity SocialMediaProviderEntity = SocialMediaProviderCreateDto.ToEntity();
            await _dbContext.SocialMediaProviders.AddAsync(SocialMediaProviderEntity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return SocialMediaProviderEntity.ToDetailDto();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhà cung cấp SOCIAL_MEDIA dựa trên ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asNoTracking"></param>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SocialMediaProviderEntity?> GetByIdAsync(Guid id,
            bool asNoTracking = true,
            bool includeDetails = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<SocialMediaProviderEntity> query = _dbContext.SocialMediaProviders;

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
        /// Cập nhật thông tin của một nhà cung cấp SOCIAL_MEDIA dựa trên ID.
        /// Nếu nhà cung cấp SOCIAL_MEDIA không tồn tại, ném ra NotFoundException;
        /// nếu tên đã tồn tại bởi nhà cung cấp khác, ném ra BusinessException.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BusinessException"></exception>
        public async Task<SocialMediaProviderDetailDto> UpdateAsync(Guid id,
            SocialMediaProviderUpdateDto request,
            CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra tồn tại
            var existingProvider = await _dbContext.SocialMediaProviders
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (existingProvider is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy SOCIAL_MEDIA provider với Id '{id}'.",
                    code: "SOCIAL_MEDIA_PROVIDER_NOT_FOUND");
            }

            // 2. Kiểm tra trùng tên (loại trừ chính nó)
            var duplicateName = await _dbContext.SocialMediaProviders.AsNoTracking()
                .AnyAsync(p => p.Name == request.Name && p.Id != id, cancellationToken);

            if (duplicateName)
            {
                throw new BusinessException(
                    message: $"SOCIAL_MEDIA provider có tên '{request.Name}' đã tồn tại.",
                    code: "SOCIAL_MEDIA_PROVIDER_ALREADY_EXISTS");
            }

            // 3. Map dữ liệu từ DTO sang entity
            request.ToEntity(existingProvider);

            // 4. Lưu thay đổi
            await _dbContext.SaveChangesAsync(cancellationToken);

            return existingProvider.ToDetailDto();
        }

        /// <summary>
        /// Xoá (soft delete) một nhà cung cấp SOCIAL_MEDIA dựa trên ID.
        /// Interceptor AuditEntityInterceptor tự động chuyển EntityState.Deleted
        /// thành Modified và set IsDeleted=true, DeletionTime, LastModificationTime.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<SocialMediaProviderDetailDto> DeleteAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            var existingProvider = await _dbContext.SocialMediaProviders
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (existingProvider is null)
            {
                throw new NotFoundException(
                    message: $"Không tìm thấy SOCIAL_MEDIA provider với Id '{id}'.",
                    code: "SOCIAL_MEDIA_PROVIDER_NOT_FOUND");
            }

            _dbContext.SocialMediaProviders.Remove(existingProvider);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return existingProvider.ToDetailDto();
        }
    }
}