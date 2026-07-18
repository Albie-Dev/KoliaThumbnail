namespace Kolia.Thumbnail.API.Engines.Providers.Socials
{
    /// <summary>
    /// Provider hỗ trợ quản lý channel/trang (YouTube channel, Facebook page, Tiktok profile...).
    /// </summary>
    public interface IChannelManagementCapableEngine : ISocialEngine
    {
        /// <summary>Lấy thông tin channel gắn với credentials OAuth hiện tại (channel của chính user).</summary>
        Task<ChannelInfo?> GetMyChannelAsync(SocialCredentials credentials, CancellationToken cancellationToken = default);

        /// <summary>Lấy thông tin 1 channel công khai theo Id.</summary>
        Task<ChannelInfo?> GetChannelByIdAsync(SocialCredentials credentials, string channelId, CancellationToken cancellationToken = default);

        /// <summary>Tìm kiếm channel theo từ khóa.</summary>
        Task<PagedResult<ChannelInfo>> SearchChannelsAsync(SocialCredentials credentials, string query, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default);

        /// <summary>Cập nhật thông tin branding (title, description, keywords...) của channel.</summary>
        Task<ChannelInfo> UpdateChannelBrandingAsync(SocialCredentials credentials, ChannelBrandingUpdateRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provider hỗ trợ quản lý video (upload, cập nhật, xóa, tìm kiếm, thumbnail, rating...).
    /// </summary>
    public interface IVideoManagementCapableEngine : ISocialEngine
    {
        /// <summary>Upload video mới (resumable upload) - hỗ trợ báo cáo tiến độ qua IProgress.</summary>
        Task<VideoInfo> UploadVideoAsync(SocialCredentials credentials, VideoUploadRequest request, IProgress<VideoUploadProgress>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>Cập nhật metadata của video đã tồn tại (title, description, tags, privacy...).</summary>
        Task<VideoInfo> UpdateVideoAsync(SocialCredentials credentials, VideoUpdateRequest request, CancellationToken cancellationToken = default);

        /// <summary>Xóa video.</summary>
        Task<bool> DeleteVideoAsync(SocialCredentials credentials, string videoId, CancellationToken cancellationToken = default);

        /// <summary>Lấy thông tin chi tiết 1 video theo Id.</summary>
        Task<VideoInfo?> GetVideoByIdAsync(SocialCredentials credentials, string videoId, CancellationToken cancellationToken = default);

        /// <summary>Liệt kê video của 1 channel (phân trang).</summary>
        Task<PagedResult<VideoInfo>> ListChannelVideosAsync(SocialCredentials credentials, VideoListRequest request, CancellationToken cancellationToken = default);

        /// <summary>Tìm kiếm video theo từ khóa, có thể giới hạn trong 1 channel.</summary>
        Task<PagedResult<VideoInfo>> SearchVideosAsync(SocialCredentials credentials, VideoSearchRequest request, CancellationToken cancellationToken = default);

        /// <summary>Đặt/cập nhật ảnh thumbnail cho video (đây là tính năng lõi của hệ thống Thumbnail).</summary>
        Task<string?> SetThumbnailAsync(SocialCredentials credentials, string videoId, byte[] imageBytes, string mimeType, CancellationToken cancellationToken = default);

        /// <summary>Like/Dislike/Bỏ đánh giá video.</summary>
        Task RateVideoAsync(SocialCredentials credentials, string videoId, VideoRatingType rating, CancellationToken cancellationToken = default);

        /// <summary>Lấy đánh giá hiện tại của user đối với video.</summary>
        Task<VideoRatingType> GetMyRatingAsync(SocialCredentials credentials, string videoId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provider hỗ trợ quản lý playlist.
    /// </summary>
    public interface IPlaylistManagementCapableEngine : ISocialEngine
    {
        Task<PlaylistInfo> CreatePlaylistAsync(SocialCredentials credentials, PlaylistCreateRequest request, CancellationToken cancellationToken = default);

        Task<PlaylistInfo> UpdatePlaylistAsync(SocialCredentials credentials, PlaylistUpdateRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeletePlaylistAsync(SocialCredentials credentials, string playlistId, CancellationToken cancellationToken = default);

        Task<PagedResult<PlaylistInfo>> ListMyPlaylistsAsync(SocialCredentials credentials, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default);

        Task<PlaylistItemInfo> AddVideoToPlaylistAsync(SocialCredentials credentials, string playlistId, string videoId, int? position = null, CancellationToken cancellationToken = default);

        Task<bool> RemovePlaylistItemAsync(SocialCredentials credentials, string playlistItemId, CancellationToken cancellationToken = default);

        Task<PagedResult<PlaylistItemInfo>> ListPlaylistItemsAsync(SocialCredentials credentials, string playlistId, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provider hỗ trợ quản lý bình luận (đọc, đăng, trả lời, kiểm duyệt, xóa).
    /// </summary>
    public interface ICommentManagementCapableEngine : ISocialEngine
    {
        Task<PagedResult<CommentThreadInfo>> ListCommentThreadsAsync(SocialCredentials credentials, string videoId, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default);

        Task<CommentThreadInfo> InsertTopLevelCommentAsync(SocialCredentials credentials, string videoId, string text, CancellationToken cancellationToken = default);

        Task<CommentInfo> ReplyToCommentAsync(SocialCredentials credentials, string parentCommentId, string text, CancellationToken cancellationToken = default);

        Task SetCommentModerationStatusAsync(SocialCredentials credentials, string commentId, CommentModerationStatus status, bool banAuthor = false, CancellationToken cancellationToken = default);

        Task<bool> DeleteCommentAsync(SocialCredentials credentials, string commentId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provider hỗ trợ livestream (tạo buổi phát, tạo luồng stream, gắn kết, chuyển trạng thái).
    /// </summary>
    public interface ILiveStreamingCapableEngine : ISocialEngine
    {
        Task<LiveBroadcastInfo> CreateLiveBroadcastAsync(SocialCredentials credentials, LiveBroadcastCreateRequest request, CancellationToken cancellationToken = default);

        Task<LiveStreamInfo> CreateLiveStreamAsync(SocialCredentials credentials, LiveStreamCreateRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gắn 1 livestream (ingest) vào 1 buổi phát đã tạo.</summary>
        Task<LiveBroadcastInfo> BindBroadcastToStreamAsync(SocialCredentials credentials, string broadcastId, string streamId, CancellationToken cancellationToken = default);

        /// <summary>Chuyển trạng thái buổi phát: testing -> live -> complete.</summary>
        Task<LiveBroadcastInfo> TransitionBroadcastAsync(SocialCredentials credentials, string broadcastId, LiveBroadcastStatus targetStatus, CancellationToken cancellationToken = default);

        Task<PagedResult<LiveBroadcastInfo>> ListMyBroadcastsAsync(SocialCredentials credentials, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default);

        Task<bool> DeleteBroadcastAsync(SocialCredentials credentials, string broadcastId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provider hỗ trợ quản lý subscription (theo dõi/hủy theo dõi channel khác).
    /// </summary>
    public interface ISubscriptionManagementCapableEngine : ISocialEngine
    {
        Task<SubscriptionInfo> SubscribeAsync(SocialCredentials credentials, string channelId, CancellationToken cancellationToken = default);

        Task<bool> UnsubscribeAsync(SocialCredentials credentials, string subscriptionId, CancellationToken cancellationToken = default);

        Task<PagedResult<SubscriptionInfo>> ListMySubscriptionsAsync(SocialCredentials credentials, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default);
    }
}
