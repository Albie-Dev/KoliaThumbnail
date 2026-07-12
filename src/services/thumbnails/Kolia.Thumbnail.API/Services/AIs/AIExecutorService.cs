using System.Runtime.CompilerServices;
using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Data.Contexts;
using Kolia.Thumbnail.API.Engines;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;
using Kolia.Thumbnail.API.Models.Engines;
using Kolia.Thumbnail.API.Security;
using Microsoft.EntityFrameworkCore;

namespace Kolia.Thumbnail.API.Services.AIs
{
    /// <summary>
    /// Dịch vụ thực thi các tác vụ AI với cơ chế fallback:
    /// - Load thông tin provider + cấu hình từ DB
    /// - Thử từng config theo Priority, fallback nếu config chính lỗi (quota, auth, 5xx...)
    /// - Nếu DB không có dữ liệu, dùng fallback default (BaseUrl + apiKey từ request)
    /// </summary>
    public sealed class AIExecutorService : IAIExecutorService
    {
        private readonly ThumbnailDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AIExecutorService> _logger;
        private readonly IApiKeyProtector _apiKeyProtector;

        // HTTP status codes có thể retry (fallback)
        private static readonly HashSet<int> RetryableStatusCodes = new()
        {
            429, // Too Many Requests (rate limit)
            401, // Unauthorized (key hết hạn/bị thu hồi)
            403, // Forbidden (quota exhausted)
            500, // Internal Server Error
            502, // Bad Gateway
            503, // Service Unavailable
            504, // Gateway Timeout
        };

        public AIExecutorService(
            ThumbnailDbContext dbContext,
            IServiceProvider serviceProvider,
            IApiKeyProtector apiKeyProtector,
            ILogger<AIExecutorService> logger)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _apiKeyProtector = apiKeyProtector;
            _logger = logger;
        }

        // ============================================================
        // Public methods
        // ============================================================

        /// <inheritdoc />
        public async Task<ProviderExecutionContext?> GetProviderContextAsync(
            CAIProviderType providerType,
            CancellationToken cancellationToken = default)
        {
            var provider = await _dbContext.AIProviders
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(p => p.Configurations)
                .FirstOrDefaultAsync(p => p.ProviderType == providerType && !p.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (provider is null)
                return null;

            var configs = provider.Configurations
                .Where(c => !c.IsDeleted && c.IsEnabled)
                .OrderBy(c => c.Priority)
                .ThenByDescending(c => c.IsDefault)
                .Select(c => new ConfigurationContext
                {
                    ConfigurationId = c.Id,
                    ApiKey = _apiKeyProtector.Unprotect(c.ApiKey),
                    ApiVersion = c.ApiVersion,
                    Priority = c.Priority,
                    IsDefault = c.IsDefault,
                    TimeoutSeconds = c.TimeoutSeconds,
                    RetryCount = c.RetryCount,
                    ExtraSettingsJson = c.ExtraSettingsJson,
                })
                .ToList();

            return new ProviderExecutionContext
            {
                ProviderId = provider.Id,
                ProviderType = provider.ProviderType,
                Name = provider.Name,
                BaseUrl = provider.BaseUrl,
                Configurations = configs,
            };
        }

        /// <inheritdoc />
        public async Task<ChatCompletionResult> ChatCompletionWithFallbackAsync(
            CAIProviderType providerType,
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            var ctx = await GetProviderContextAsync(providerType, cancellationToken).ConfigureAwait(false);
            var engine = ResolveEngine<IChatCapableEngine>(providerType);

            return await ExecuteWithFallbackAsync(
                ctx, providerType,
                (config, ct) =>
                {
                    ApplyConfigToRequest(request, config, ctx);
                    return engine.ChatCompletionAsync(request);
                },
                request.ApiKey,
                cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ChatCompletionChunk> ChatCompletionStreamWithFallbackAsync(
            CAIProviderType providerType,
            ChatCompletionRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var ctx = await GetProviderContextAsync(providerType, cancellationToken).ConfigureAwait(false);
            var engine = ResolveEngine<IChatCapableEngine>(providerType);

            var configs = GetConfigList(ctx, request.ApiKey);
            bool anyAttempted = false;

            foreach (var config in configs)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                ApplyConfigToRequest(request, config, ctx);
                anyAttempted = true;

                var enumerator = await TryStartStreamAsync(engine, request, cancellationToken).ConfigureAwait(false);

                if (enumerator is not null)
                {
                    await using (enumerator.ConfigureAwait(false))
                    {
                        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                        {
                            yield return enumerator.Current;
                        }
                    }
                    yield break;
                }

                _logger.LogWarning(
                    "Streaming failed for {ProviderType} config {ConfigId}, trying next config.",
                    providerType, config.ConfigurationId);
            }

            if (!anyAttempted)
            {
                await foreach (var chunk in engine.ChatCompletionStreamAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    yield return chunk;
                }
            }
        }

        /// <summary>
        /// Thử lấy async enumerator cho stream. Trả về null nếu request thất bại ngay lập tức.
        /// </summary>
        private static async Task<IAsyncEnumerator<ChatCompletionChunk>?> TryStartStreamAsync(
            IChatCapableEngine engine,
            ChatCompletionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var stream = engine.ChatCompletionStreamAsync(request, cancellationToken);
                var enumerator = stream.GetAsyncEnumerator(cancellationToken);

                if (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    return enumerator;
                }

                // Stream rỗng (0 chunk) - vẫn trả về enumerator
                return enumerator;
            }
            catch (AiProviderException ex) when (IsRetryable(ex.ProviderStatusCode))
            {
                return null;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<ImageGenerationResult> GenerateImageWithFallbackAsync(
            CAIProviderType providerType,
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            var ctx = await GetProviderContextAsync(providerType, cancellationToken).ConfigureAwait(false);
            var engine = ResolveEngine<IImageGenerationCapableEngine>(providerType);

            return await ExecuteWithFallbackAsync(
                ctx, providerType,
                (config, ct) =>
                {
                    ApplyConfigToRequest(request, config, ctx);
                    return engine.GenerateImageAsync(request);
                },
                request.ApiKey,
                cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TextToSpeechResult> GenerateSpeechWithFallbackAsync(
            CAIProviderType providerType,
            TextToSpeechRequest request,
            CancellationToken cancellationToken = default)
        {
            var ctx = await GetProviderContextAsync(providerType, cancellationToken).ConfigureAwait(false);
            var engine = ResolveEngine<ITextToSpeechCapableEngine>(providerType);

            return await ExecuteWithFallbackAsync(
                ctx, providerType,
                (config, ct) =>
                {
                    ApplyConfigToRequest(request, config, ctx);
                    return engine.GenerateSpeechAsync(request);
                },
                request.ApiKey,
                cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<SpeechToTextResult> TranscribeWithFallbackAsync(
            CAIProviderType providerType,
            SpeechToTextRequest request,
            CancellationToken cancellationToken = default)
        {
            var ctx = await GetProviderContextAsync(providerType, cancellationToken).ConfigureAwait(false);
            var engine = ResolveEngine<ISpeechToTextCapableEngine>(providerType);

            return await ExecuteWithFallbackAsync(
                ctx, providerType,
                async (config, ct) =>
                {
                    ApplyConfigToRequest(request, config, ctx);
                    return await engine.TranscribeAudioAsync(request).ConfigureAwait(false);
                },
                request.ApiKey,
                cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<EmbeddingResult> CreateEmbeddingWithFallbackAsync(
            CAIProviderType providerType,
            EmbeddingRequest request,
            CancellationToken cancellationToken = default)
        {
            var ctx = await GetProviderContextAsync(providerType, cancellationToken).ConfigureAwait(false);
            var engine = ResolveEngine<IEmbeddingCapableEngine>(providerType);

            return await ExecuteWithFallbackAsync(
                ctx, providerType,
                (config, ct) =>
                {
                    ApplyConfigToRequest(request, config, ctx);
                    return engine.CreateEmbeddingAsync(request);
                },
                request.ApiKey,
                cancellationToken).ConfigureAwait(false);
        }

        // ============================================================
        // Private helpers
        // ============================================================

        /// <summary>
        /// Lấy danh sách config cần thử, gồm:
        /// 1. Config từ DB (nếu có) sắp xếp theo Priority
        /// 2. Fallback: apiKey gốc từ request (dùng khi DB không có config)
        /// </summary>
        private List<ConfigurationContext> GetConfigList(ProviderExecutionContext? ctx, string fallbackApiKey)
        {
            var result = new List<ConfigurationContext>();

            if (ctx?.Configurations is { Count: > 0 })
            {
                result.AddRange(ctx.Configurations);
            }

            // Fallback: nếu request có apiKey riêng và không trùng với config nào
            if (!string.IsNullOrWhiteSpace(fallbackApiKey))
            {
                var hasDuplicate = result.Any(c =>
                    string.Equals(c.ApiKey, fallbackApiKey, StringComparison.Ordinal));

                if (!hasDuplicate)
                {
                    result.Add(new ConfigurationContext
                    {
                        ConfigurationId = Guid.Empty,
                        ApiKey = fallbackApiKey,
                        Priority = int.MaxValue, // Luôn thử cuối cùng
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Thực thi một tác vụ AI với fallback qua các config.
        /// </summary>
        private async Task<TResult> ExecuteWithFallbackAsync<TResult>(
            ProviderExecutionContext? ctx,
            CAIProviderType providerType,
            Func<ConfigurationContext, CancellationToken, Task<TResult>> execute,
            string fallbackApiKey,
            CancellationToken cancellationToken)
        {
            var configs = GetConfigList(ctx, fallbackApiKey);

            if (configs.Count == 0)
            {
                // Không có config nào → dùng fallback default (BaseUrl từ provider nếu có)
                var fallbackConfig = new ConfigurationContext
                {
                    ConfigurationId = Guid.Empty,
                    ApiKey = fallbackApiKey,
                    Priority = int.MaxValue,
                };

                return await execute(fallbackConfig, cancellationToken).ConfigureAwait(false);
            }

            List<Exception> errors = new();

            foreach (var config in configs)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    return await execute(config, cancellationToken).ConfigureAwait(false);
                }
                catch (AiProviderException ex) when (IsRetryable(ex.ProviderStatusCode))
                {
                    errors.Add(ex);
                    _logger.LogWarning(ex,
                        "AI call failed for {ProviderType} config {ConfigId} (HTTP {StatusCode}, code: {ErrorCode}), trying next config. Attempt {Attempt}/{Total}.",
                        providerType, config.ConfigurationId, ex.ProviderStatusCode, ex.ProviderErrorCode ?? "N/A",
                        errors.Count, configs.Count);
                }
                catch (HttpRequestException ex)
                {
                    errors.Add(ex);
                    _logger.LogWarning(ex,
                        "AI call failed for {ProviderType} config {ConfigId} (network error), trying next config. Attempt {Attempt}/{Total}.",
                        providerType, config.ConfigurationId, errors.Count, configs.Count);
                }
                catch (TaskCanceledException)
                {
                    // Timeout — có thể thử config khác
                    errors.Add(new TimeoutException($"Request timed out for {providerType} config {config.ConfigurationId}."));
                    _logger.LogWarning(
                        "AI call timed out for {ProviderType} config {ConfigId}, trying next config. Attempt {Attempt}/{Total}.",
                        providerType, config.ConfigurationId, errors.Count, configs.Count);
                }
            }

            // Tất cả đều thất bại
            _logger.LogError("All {Total} config(s) failed for {ProviderType}. Errors: {ErrorCount}",
                configs.Count, providerType, errors.Count);

            throw new AggregateException(
                $"Tất cả {configs.Count} cấu hình cho {providerType} đều thất bại. " +
                $"Đã thử {errors.Count} lần, vui lòng kiểm tra lại API key hoặc thử lại sau.",
                errors);
        }

        /// <summary>
        /// Áp dụng config (BaseUrl, ApiKey) vào request DTO.
        /// </summary>
        private static void ApplyConfigToRequest<T>(T request, ConfigurationContext config, ProviderExecutionContext? ctx)
            where T : class
        {
            // Gán ApiKey
            SetProperty(request, "ApiKey", config.ApiKey);

            // Nếu config có BaseUrl riêng, ưu tiên dùng
            var effectiveBaseUrl = config.BaseUrl ?? ctx?.BaseUrl;
            if (!string.IsNullOrWhiteSpace(effectiveBaseUrl))
            {
                SetProperty(request, "BaseUrl", effectiveBaseUrl);
            }
        }

        /// <summary>
        /// Resolve engine từ DI. Dùng IServiceProvider để lấy đúng instance.
        /// </summary>
        private TEngine ResolveEngine<TEngine>(CAIProviderType providerType) where TEngine : class, IAIEngine
        {
            // Lấy tất cả engine đã đăng ký, tìm cái phù hợp
            var engines = _serviceProvider.GetServices<TEngine>();
            var engine = engines.FirstOrDefault(e => e.ProviderType == providerType);

            if (engine is null)
            {
                throw new NotSupportedException(
                    $"Không tìm thấy engine cho provider '{providerType}'. " +
                    $"Vui lòng đảm bảo engine đã được đăng ký trong DI.");
            }

            return engine;
        }

        /// <summary>
        /// Kiểm tra HTTP status code có thể fallback sang config khác không.
        /// </summary>
        private static bool IsRetryable(int statusCode)
        {
            return RetryableStatusCodes.Contains(statusCode);
        }

        /// <summary>
        /// Set property bằng reflection (dùng cho các request DTO có property ApiKey/BaseUrl).
        /// </summary>
        private static void SetProperty<T>(T target, string propertyName, string value)
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop is not null && prop.CanWrite)
            {
                prop.SetValue(target, value);
            }
        }
    }
}
