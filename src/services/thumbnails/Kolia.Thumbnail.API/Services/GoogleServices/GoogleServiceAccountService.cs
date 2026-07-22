using System.Text.Json;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Data.Entities.GoogleServices;
using Kolia.Thumbnail.API.DTOs.GoogleServices;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Extensions;
using Kolia.Thumbnail.API.Interfaces.GoogleServices;
using Kolia.Thumbnail.API.Models.Commons;
using Kolia.Thumbnail.API.Security;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.GoogleServices
{
    public class GoogleServiceAccountService : IGoogleServiceAccountService
    {
        private readonly ThumbnailDbContext _db;
        private readonly IApiKeyProtector _protector;
        private readonly ILogger<GoogleServiceAccountService> _logger;

        public GoogleServiceAccountService(
            ThumbnailDbContext db,
            IApiKeyProtector protector,
            ILogger<GoogleServiceAccountService> logger)
        {
            _db = db;
            _protector = protector;
            _logger = logger;
        }

        public async Task<PagedResponseDto<GoogleServiceAccountSummaryDto>> GetWithPagingAsync(
            PagedRequestDto request,
            bool? includeDeleted = null,
            bool? deletedOnly = null,
            CancellationToken ct = default)
        {
            IQueryable<GoogleServiceAccountEntity> query = _db.Set<GoogleServiceAccountEntity>()
                .AsNoTracking();

            if (includeDeleted.HasValue)
            {
                query = query.IgnoreQueryFilters();
                if (deletedOnly == true)
                    query = query.Where(x => x.IsDeleted);
                else
                    query = query.Where(x => !x.IsDeleted);
            }
            else
            {
                query = query.Where(x => !x.IsDeleted);
            }

            query = query.ApplyQuery(request);

            var jobCounts = await _db.Set<ScheduledImportJobEntity>()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.GoogleServiceAccountId)
                .Select(g => new { ServiceAccountId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ServiceAccountId, x => x.Count, ct);

            return await query.ToPagedResponseAsync<GoogleServiceAccountEntity, GoogleServiceAccountSummaryDto>(
                request,
                selector: e => new GoogleServiceAccountSummaryDto(
                    e.Id,
                    e.Name,
                    e.ClientEmail,
                    e.ProjectId,
                    e.IsEnabled,
                    jobCounts.TryGetValue(e.Id, out var count) ? count : 0,
                    e.CreationTime),
                cancellationToken: ct);
        }

        public async Task<GoogleServiceAccountDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<GoogleServiceAccountEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null) return null;

            return MapToDto(entity);
        }

        public async Task<GoogleServiceAccountDto> CreateAsync(
            CreateGoogleServiceAccountRequest request,
            CancellationToken ct = default)
        {
            // Parse JSON credential
            JsonElement json;
            try
            {
                json = JsonSerializer.Deserialize<JsonElement>(request.CredentialJson);
            }
            catch (JsonException)
            {
                throw new ValidationException("File JSON credential không hợp lệ.");
            }

            var clientEmail = json.TryGetProperty("client_email", out var email)
                ? email.GetString() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(clientEmail))
                throw new ValidationException("File JSON thiếu trường 'client_email'.");

            // Kiểm tra trùng client_email
            var exists = await _db.Set<GoogleServiceAccountEntity>()
                .AnyAsync(x => x.ClientEmail == clientEmail && !x.IsDeleted, ct);
            if (exists)
                throw new BusinessException($"Service account với email '{clientEmail}' đã tồn tại.");

            var privateKey = json.TryGetProperty("private_key", out var pk)
                ? pk.GetString() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ValidationException("File JSON thiếu trường 'private_key'.");

            var privateKeyId = json.TryGetProperty("private_key_id", out var pkId)
                ? pkId.GetString()
                : null;

            var entity = new GoogleServiceAccountEntity
            {
                Name = request.Name,
                Description = request.Description,
                ClientEmail = clientEmail,
                ClientId = json.TryGetProperty("client_id", out var cid) ? cid.GetString() : null,
                ProjectId = json.TryGetProperty("project_id", out var pid) ? pid.GetString() : null,
                TokenUri = json.TryGetProperty("token_uri", out var tu) ? tu.GetString() : null,
                AuthUri = json.TryGetProperty("auth_uri", out var au) ? au.GetString() : null,
                AuthProviderX509CertUrl = json.TryGetProperty("auth_provider_x509_cert_url", out var acu) ? acu.GetString() : null,
                PrivateKeyIdHash = privateKeyId != null ? _protector.Hash(privateKeyId) : null,
                PrivateKey = _protector.Protect(privateKey),
                RawCredentialJson = _protector.Protect(request.CredentialJson),
                CredentialJsonHash = _protector.Hash(request.CredentialJson),
                Scopes = request.Scopes,
                IsEnabled = true
            };

            _db.Set<GoogleServiceAccountEntity>().Add(entity);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Created GoogleServiceAccount: {Name} ({Email})", entity.Name, entity.ClientEmail);

            return MapToDto(entity);
        }

        public async Task<GoogleServiceAccountDto> UpdateAsync(
            Guid id,
            UpdateGoogleServiceAccountRequest request,
            CancellationToken ct = default)
        {
            var entity = await _db.Set<GoogleServiceAccountEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy service account với ID: {id}");

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Scopes = request.Scopes;
            entity.IsEnabled = request.IsEnabled;

            // Nếu có cập nhật credential JSON mới
            if (!string.IsNullOrWhiteSpace(request.CredentialJson))
            {
                JsonElement json;
                try
                {
                    json = JsonSerializer.Deserialize<JsonElement>(request.CredentialJson);
                }
                catch (JsonException)
                {
                    throw new ValidationException("File JSON credential không hợp lệ.");
                }

                var newHash = _protector.Hash(request.CredentialJson);

                // Chỉ update nếu JSON thay đổi
                if (entity.CredentialJsonHash != newHash)
                {
                    var clientEmail = json.TryGetProperty("client_email", out var email)
                        ? email.GetString() ?? string.Empty
                        : string.Empty;

                    if (string.IsNullOrWhiteSpace(clientEmail))
                        throw new ValidationException("File JSON thiếu trường 'client_email'.");

                    var privateKey = json.TryGetProperty("private_key", out var pk)
                        ? pk.GetString() ?? string.Empty
                        : string.Empty;

                    if (string.IsNullOrWhiteSpace(privateKey))
                        throw new ValidationException("File JSON thiếu trường 'private_key'.");

                    var privateKeyId = json.TryGetProperty("private_key_id", out var pkId)
                        ? pkId.GetString()
                        : null;

                    entity.ClientEmail = clientEmail;
                    entity.ClientId = json.TryGetProperty("client_id", out var cid) ? cid.GetString() : null;
                    entity.ProjectId = json.TryGetProperty("project_id", out var pid) ? pid.GetString() : null;
                    entity.TokenUri = json.TryGetProperty("token_uri", out var tu) ? tu.GetString() : null;
                    entity.AuthUri = json.TryGetProperty("auth_uri", out var au) ? au.GetString() : null;
                    entity.AuthProviderX509CertUrl = json.TryGetProperty("auth_provider_x509_cert_url", out var acu) ? acu.GetString() : null;
                    entity.PrivateKeyIdHash = privateKeyId != null ? _protector.Hash(privateKeyId) : null;
                    entity.PrivateKey = _protector.Protect(privateKey);
                    entity.RawCredentialJson = _protector.Protect(request.CredentialJson);
                    entity.CredentialJsonHash = newHash;
                }
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Updated GoogleServiceAccount: {Name} ({Email})", entity.Name, entity.ClientEmail);

            return MapToDto(entity);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Set<GoogleServiceAccountEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
                ?? throw new NotFoundException($"Không tìm thấy service account với ID: {id}");

            // Kiểm tra có job đang chạy không
            var hasRunningJobs = await _db.Set<ScheduledImportJobEntity>()
                .AnyAsync(x => x.GoogleServiceAccountId == id
                            && x.Status == Enums.CJobScheduleStatus.Pending
                            && !x.IsDeleted, ct);

            if (hasRunningJobs)
                throw new BusinessException("Không thể xoá service account vì còn job đang chờ xử lý.");
            
            _db.Remove(entity);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted GoogleServiceAccount: {Name} ({Email})", entity.Name, entity.ClientEmail);
        }

        private static GoogleServiceAccountDto MapToDto(GoogleServiceAccountEntity e)
            => new(
                e.Id,
                e.Name,
                e.Description,
                e.ClientEmail,
                e.ClientId,
                e.ProjectId,
                e.TokenUri,
                e.Scopes,
                e.IsEnabled,
                e.CreationTime,
                e.LastModificationTime);
    }
}
