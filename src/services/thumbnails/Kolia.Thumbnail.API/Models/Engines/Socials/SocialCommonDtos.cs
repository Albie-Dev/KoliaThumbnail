namespace Kolia.Thumbnail.API.Engines.Providers.Socials
{
    // ============================================================
    // Credentials & Capabilities
    // ============================================================

    /// <summary>
    /// Bộ thông tin xác thực dùng chung cho mọi Social Media Provider.
    /// Không phải provider nào cũng dùng hết các trường - engine cụ thể sẽ
    /// chỉ đọc những trường nó cần (VD: Youtube cần ClientId/ClientSecret/RefreshToken/AccessToken,
    /// một số thao tác đọc công khai chỉ cần ApiKey).
    /// </summary>
    public sealed class SocialCredentials
    {
        /// <summary>API Key (dùng cho các request chỉ đọc dữ liệu công khai, không cần OAuth).</summary>
        public string? ApiKey { get; set; }

        /// <summary>OAuth2 Client Id đã đăng ký với provider.</summary>
        public string? ClientId { get; set; }

        /// <summary>OAuth2 Client Secret đã đăng ký với provider.</summary>
        public string? ClientSecret { get; set; }

        /// <summary>OAuth2 Access Token hiện tại (có thể đã hết hạn).</summary>
        public string? AccessToken { get; set; }

        /// <summary>OAuth2 Refresh Token, dùng để tự động làm mới Access Token.</summary>
        public string? RefreshToken { get; set; }

        /// <summary>Danh sách scope OAuth đã cấp, phân tách bởi khoảng trắng.</summary>
        public string? Scope { get; set; }

        /// <summary>Id cấu hình (SocialMediaProviderConfigurationEntity.Id) - phục vụ logging/theo dõi.</summary>
        public Guid? ConfigurationId { get; set; }

        public bool HasOAuthToken => !string.IsNullOrWhiteSpace(AccessToken) || !string.IsNullOrWhiteSpace(RefreshToken);

        public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);
    }

    /// <summary>
    /// Khai báo các nhóm năng lực mà 1 social engine hỗ trợ, dùng để hệ thống
    /// biết cast ISocialEngine sang interface con phù hợp (tương tự AIProviderCapabilities).
    /// </summary>
    public sealed class SocialProviderCapabilities
    {
        public bool SupportsChannelManagement { get; set; }
        public bool SupportsVideoManagement { get; set; }
        public bool SupportsPlaylistManagement { get; set; }
        public bool SupportsCommentManagement { get; set; }
        public bool SupportsLiveStreaming { get; set; }
        public bool SupportsSubscriptionManagement { get; set; }
    }

    /// <summary>
    /// Kết quả phân trang chung, tương thích với cơ chế pageToken kiểu YouTube Data API
    /// cũng như cursor-based paging của các provider khác (Facebook, TikTok...).
    /// </summary>
    public sealed class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public string? NextPageToken { get; set; }
        public string? PrevPageToken { get; set; }
        public long? TotalResults { get; set; }
    }

    // ============================================================
    // Channel
    // ============================================================

    public sealed class ChannelInfo
    {
        public string ChannelId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string? CustomUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public long? SubscriberCount { get; set; }
        public long? VideoCount { get; set; }
        public long? ViewCount { get; set; }
        public bool HiddenSubscriberCount { get; set; }
        public string? UploadsPlaylistId { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public string? Country { get; set; }
    }

    public sealed class ChannelBrandingUpdateRequest
    {
        public string ChannelId { get; set; } = default!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? Keywords { get; set; }
    }

    // ============================================================
    // Video
    // ============================================================

    public enum VideoPrivacyStatus
    {
        Private = 0,
        Unlisted = 1,
        Public = 2
    }

    public enum VideoRatingType
    {
        None = 0,
        Like = 1,
        Dislike = 2
    }

    public sealed class VideoUploadRequest
    {
        /// <summary>Luồng dữ liệu video (mp4, mov...). Bắt buộc, engine sẽ đọc và upload dạng resumable.</summary>
        public Stream VideoStream { get; set; } = default!;
        public string MimeType { get; set; } = "video/*";
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public List<string>? Tags { get; set; }
        public string? CategoryId { get; set; }
        public VideoPrivacyStatus PrivacyStatus { get; set; } = VideoPrivacyStatus.Private;
        public bool MadeForKids { get; set; }
        public DateTimeOffset? PublishAt { get; set; }
        public string? DefaultLanguage { get; set; }

        /// <summary>Nếu có, engine sẽ tự động gọi SetThumbnailAsync ngay sau khi upload xong.</summary>
        public byte[]? ThumbnailBytes { get; set; }
        public string? ThumbnailMimeType { get; set; }
    }

    public sealed class VideoUpdateRequest
    {
        public string VideoId { get; set; } = default!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string>? Tags { get; set; }
        public string? CategoryId { get; set; }
        public VideoPrivacyStatus? PrivacyStatus { get; set; }
        public bool? MadeForKids { get; set; }
        public DateTimeOffset? PublishAt { get; set; }
    }

    public sealed class VideoUploadProgress
    {
        public long BytesSent { get; set; }
        public long? TotalBytes { get; set; }
        public string Status { get; set; } = default!;
    }

    public sealed class VideoInfo
    {
        public string VideoId { get; set; } = default!;
        public string ChannelId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? CategoryId { get; set; }
        public VideoPrivacyStatus PrivacyStatus { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? UploadStatus { get; set; }
        public string? ProcessingStatus { get; set; }
        public TimeSpan? Duration { get; set; }
        public long? ViewCount { get; set; }
        public long? LikeCount { get; set; }
        public long? CommentCount { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }

    public sealed class VideoListRequest
    {
        public string ChannelId { get; set; } = default!;
        public int MaxResults { get; set; } = 25;
        public string? PageToken { get; set; }
    }

    public sealed class VideoSearchRequest
    {
        public string Query { get; set; } = default!;
        public string? ChannelId { get; set; }
        public int MaxResults { get; set; } = 25;
        public string? PageToken { get; set; }
        public string Order { get; set; } = "relevance"; // date, rating, relevance, title, viewCount
    }

    // ============================================================
    // Playlist
    // ============================================================

    public sealed class PlaylistInfo
    {
        public string PlaylistId { get; set; } = default!;
        public string ChannelId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public VideoPrivacyStatus PrivacyStatus { get; set; }
        public long? ItemCount { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public sealed class PlaylistCreateRequest
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public VideoPrivacyStatus PrivacyStatus { get; set; } = VideoPrivacyStatus.Private;
        public List<string>? Tags { get; set; }
    }

    public sealed class PlaylistUpdateRequest
    {
        public string PlaylistId { get; set; } = default!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public VideoPrivacyStatus? PrivacyStatus { get; set; }
    }

    public sealed class PlaylistItemInfo
    {
        public string PlaylistItemId { get; set; } = default!;
        public string PlaylistId { get; set; } = default!;
        public string VideoId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public int Position { get; set; }
        public DateTimeOffset? AddedAt { get; set; }
    }

    // ============================================================
    // Comment
    // ============================================================

    public enum CommentModerationStatus
    {
        Published = 0,
        HeldForReview = 1,
        Rejected = 2,
        Spam = 3
    }

    public sealed class CommentInfo
    {
        public string CommentId { get; set; } = default!;
        public string AuthorDisplayName { get; set; } = default!;
        public string? AuthorChannelId { get; set; }
        public string TextDisplay { get; set; } = default!;
        public long LikeCount { get; set; }
        public bool CanRate { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public sealed class CommentThreadInfo
    {
        public string CommentThreadId { get; set; } = default!;
        public string VideoId { get; set; } = default!;
        public CommentInfo TopLevelComment { get; set; } = default!;
        public long ReplyCount { get; set; }
        public bool IsPublic { get; set; }
        public List<CommentInfo> Replies { get; set; } = new();
    }

    // ============================================================
    // Live Streaming
    // ============================================================

    public enum LiveBroadcastStatus
    {
        Created = 0,
        Ready = 1,
        Testing = 2,
        Live = 3,
        Complete = 4,
        Revoked = 5
    }

    public sealed class LiveBroadcastCreateRequest
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTimeOffset ScheduledStartTime { get; set; }
        public DateTimeOffset? ScheduledEndTime { get; set; }
        public VideoPrivacyStatus PrivacyStatus { get; set; } = VideoPrivacyStatus.Private;
        public bool EnableAutoStart { get; set; } = true;
        public bool EnableAutoStop { get; set; } = true;
        public bool EnableDvr { get; set; } = true;
        public bool MadeForKids { get; set; }
    }

    public sealed class LiveBroadcastInfo
    {
        public string BroadcastId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public LiveBroadcastStatus Status { get; set; }
        public DateTimeOffset? ScheduledStartTime { get; set; }
        public DateTimeOffset? ScheduledEndTime { get; set; }
        public DateTimeOffset? ActualStartTime { get; set; }
        public DateTimeOffset? ActualEndTime { get; set; }
        public string? BoundStreamId { get; set; }
    }

    public sealed class LiveStreamCreateRequest
    {
        public string Title { get; set; } = default!;
        public string Resolution { get; set; } = "1080p"; // 240p..1080p, variable
        public string FrameRate { get; set; } = "30fps"; // 30fps, 60fps, variable
        public string IngestionType { get; set; } = "rtmp"; // rtmp, hls, dash
    }

    public sealed class LiveStreamInfo
    {
        public string StreamId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? IngestionAddress { get; set; }
        public string? StreamKey { get; set; }
        public string? Status { get; set; }
    }

    // ============================================================
    // Subscription
    // ============================================================

    public sealed class SubscriptionInfo
    {
        public string SubscriptionId { get; set; } = default!;
        public string ChannelId { get; set; } = default!;
        public string ChannelTitle { get; set; } = default!;
        public DateTimeOffset? SubscribedAt { get; set; }
    }
}
