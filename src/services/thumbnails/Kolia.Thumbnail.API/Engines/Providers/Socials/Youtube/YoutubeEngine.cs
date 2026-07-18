using Google;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Kolia.Thumbnail.API.Enums;
using Kolia.Thumbnail.API.Exceptions;

using YTData = Google.Apis.YouTube.v3.Data;

namespace Kolia.Thumbnail.API.Engines.Providers.Socials.Youtube
{
    /// <summary>
    /// Engine cho YouTube (https://developers.google.com/youtube/v3), triển khai qua
    /// Google.Apis.YouTube.v3 - thư viện chính thức của Google.
    ///
    /// YouTube hỗ trợ đầy đủ: quản lý channel, quản lý video (upload resumable, cập nhật, xóa,
    /// tìm kiếm, đặt thumbnail, rating), quản lý playlist, quản lý bình luận, livestream và
    /// subscription. Toàn bộ thao tác ghi/đọc dữ liệu riêng tư bắt buộc dùng OAuth2
    /// (ClientId/ClientSecret/AccessToken/RefreshToken); thao tác đọc dữ liệu công khai có thể
    /// dùng ApiKey - xem <see cref="YoutubeCredentialFactory"/>.
    /// </summary>
    public sealed class YoutubeEngine :
        SocialEngine,
        IChannelManagementCapableEngine,
        IVideoManagementCapableEngine,
        IPlaylistManagementCapableEngine,
        ICommentManagementCapableEngine,
        ILiveStreamingCapableEngine,
        ISubscriptionManagementCapableEngine
    {
        private const string ChannelParts = "snippet,contentDetails,statistics,brandingSettings";
        private const string VideoParts = "snippet,status,statistics,contentDetails,processingDetails";
        private const string PlaylistParts = "snippet,status,contentDetails";
        private const string PlaylistItemParts = "snippet,contentDetails";
        private const string CommentThreadParts = "snippet,replies";
        private const string CommentParts = "snippet";
        private const string LiveBroadcastParts = "snippet,status,contentDetails";
        private const string LiveStreamParts = "snippet,cdn,status";
        private const string SubscriptionParts = "snippet";

        private readonly ILogger<YoutubeEngine> _logger;

        public YoutubeEngine(ILogger<YoutubeEngine> logger)
        {
            _logger = logger;
        }

        public override CSocialMediaProviderType ProviderType => CSocialMediaProviderType.Youtube;

        // ============================================================
        // ISocialEngine - phần chung
        // ============================================================

        public override SocialProviderCapabilities GetCapabilities() => new()
        {
            SupportsChannelManagement = true,
            SupportsVideoManagement = true,
            SupportsPlaylistManagement = true,
            SupportsCommentManagement = true,
            SupportsLiveStreaming = true,
            SupportsSubscriptionManagement = true
        };

        public override async Task<bool> ValidateCredentialsAsync(SocialCredentials credentials, CancellationToken cancellationToken = default)
        {
            try
            {
                using var service = CreateService(credentials);

                if (YoutubeCredentialFactory.HasOAuthCapability(credentials))
                {
                    var request = service.Channels.List(ChannelParts);
                    request.Mine = true;
                    await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var request = service.I18nRegions.List("snippet");
                    await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                }

                return true;
            }
            catch (GoogleApiException)
            {
                return false;
            }
            catch (Exception ex) when (ex is not ValidationException)
            {
                _logger.LogWarning(ex, "Không thể xác thực credentials YouTube.");
                return false;
            }
        }

        public override async Task<SocialCredentials> RefreshCredentialsAsync(SocialCredentials credentials, CancellationToken cancellationToken = default)
        {
            var token = await YoutubeCredentialFactory.RefreshAccessTokenAsync(credentials, cancellationToken).ConfigureAwait(false);

            return new SocialCredentials
            {
                ApiKey = credentials.ApiKey,
                ClientId = credentials.ClientId,
                ClientSecret = credentials.ClientSecret,
                AccessToken = token.AccessToken,
                // Google chỉ trả RefreshToken mới khi có thay đổi (thường không) - giữ lại token cũ nếu response không trả về.
                RefreshToken = string.IsNullOrEmpty(token.RefreshToken) ? credentials.RefreshToken : token.RefreshToken,
                Scope = string.IsNullOrEmpty(token.Scope) ? credentials.Scope : token.Scope,
                ConfigurationId = credentials.ConfigurationId
            };
        }

        // ============================================================
        // IChannelManagementCapableEngine
        // ============================================================

        public async Task<ChannelInfo?> GetMyChannelAsync(SocialCredentials credentials, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Channels.List(ChannelParts);
            request.Mine = true;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(GetMyChannelAsync)).ConfigureAwait(false);
            return response.Items?.Select(MapChannel).FirstOrDefault();
        }

        public async Task<ChannelInfo?> GetChannelByIdAsync(SocialCredentials credentials, string channelId, CancellationToken cancellationToken = default)
        {
            using var service = CreateService(credentials);
            var request = service.Channels.List(ChannelParts);
            request.Id = channelId;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(GetChannelByIdAsync)).ConfigureAwait(false);
            return response.Items?.Select(MapChannel).FirstOrDefault();
        }

        public async Task<PagedResult<ChannelInfo>> SearchChannelsAsync(SocialCredentials credentials, string query, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default)
        {
            using var service = CreateService(credentials);
            var request = service.Search.List("snippet");
            request.Q = query;
            request.Type = "channel";
            request.MaxResults = maxResults;
            request.PageToken = pageToken;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(SearchChannelsAsync)).ConfigureAwait(false);

            return new PagedResult<ChannelInfo>
            {
                Items = response.Items?.Select(i => new ChannelInfo
                {
                    ChannelId = i.Id.ChannelId,
                    Title = i.Snippet.Title,
                    Description = i.Snippet.Description,
                    ThumbnailUrl = i.Snippet.Thumbnails?.High?.Url ?? i.Snippet.Thumbnails?.Default__?.Url,
                    PublishedAt = i.Snippet.PublishedAtDateTimeOffset
                }).ToList() ?? new List<ChannelInfo>(),
                NextPageToken = response.NextPageToken,
                PrevPageToken = response.PrevPageToken,
                TotalResults = response.PageInfo?.TotalResults
            };
        }

        public async Task<ChannelInfo> UpdateChannelBrandingAsync(SocialCredentials credentials, ChannelBrandingUpdateRequest request, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var getRequest = service.Channels.List(ChannelParts);
            getRequest.Id = request.ChannelId;
            var existing = await ExecuteAsync(() => getRequest.ExecuteAsync(cancellationToken), nameof(UpdateChannelBrandingAsync)).ConfigureAwait(false);
            var channel = existing.Items?.FirstOrDefault()
                ?? throw new NotFoundException($"Không tìm thấy YouTube channel với Id '{request.ChannelId}'.", "SOCIAL_YOUTUBE_CHANNEL_NOT_FOUND");

            channel.BrandingSettings ??= new YTData.ChannelBrandingSettings();
            channel.BrandingSettings.Channel ??= new YTData.ChannelSettings();

            if (request.Title is not null)
            {
                channel.BrandingSettings.Channel.Title = request.Title;
                channel.Snippet.Title = request.Title;
            }

            if (request.Description is not null)
            {
                channel.BrandingSettings.Channel.Description = request.Description;
                channel.Snippet.Description = request.Description;
            }

            if (request.Country is not null)
                channel.BrandingSettings.Channel.Country = request.Country;

            if (request.Keywords is not null)
                channel.BrandingSettings.Channel.Keywords = request.Keywords;

            var updateRequest = service.Channels.Update(channel, "brandingSettings,snippet");
            var updated = await ExecuteAsync(() => updateRequest.ExecuteAsync(cancellationToken), nameof(UpdateChannelBrandingAsync)).ConfigureAwait(false);
            return MapChannel(updated);
        }

        // ============================================================
        // IVideoManagementCapableEngine
        // ============================================================

        public async Task<VideoInfo> UploadVideoAsync(
            SocialCredentials credentials,
            VideoUploadRequest request,
            IProgress<VideoUploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var video = new YTData.Video
            {
                Snippet = new YTData.VideoSnippet
                {
                    Title = request.Title,
                    Description = request.Description,
                    Tags = request.Tags,
                    CategoryId = request.CategoryId,
                    DefaultLanguage = request.DefaultLanguage
                },
                Status = new YTData.VideoStatus
                {
                    PrivacyStatus = MapPrivacyStatus(request.PrivacyStatus),
                    SelfDeclaredMadeForKids = request.MadeForKids,
                    PublishAtDateTimeOffset = request.PublishAt
                }
            };

            var insertRequest = service.Videos.Insert(video, "snippet,status", request.VideoStream, request.MimeType);

            Exception? uploadFailure = null;

            insertRequest.ProgressChanged += p =>
            {
                if (p.Status == UploadStatus.Failed)
                    uploadFailure = p.Exception;

                progress?.Report(new VideoUploadProgress
                {
                    BytesSent = p.BytesSent,
                    TotalBytes = request.VideoStream.CanSeek ? request.VideoStream.Length : null,
                    Status = p.Status.ToString()
                });
            };

            YTData.Video? uploadedVideo = null;
            insertRequest.ResponseReceived += v => uploadedVideo = v;

            var finalProgress = await ExecuteAsync(
                () => insertRequest.UploadAsync(cancellationToken),
                nameof(UploadVideoAsync)).ConfigureAwait(false);

            if (finalProgress.Status == UploadStatus.Failed || uploadedVideo is null)
            {
                var failure = uploadFailure ?? finalProgress.Exception;
                throw new SocialProviderException(
                    CSocialMediaProviderType.Youtube,
                    StatusCodes.Status502BadGateway,
                    failure?.Message ?? "Upload video lên YouTube thất bại không rõ nguyên nhân.",
                    innerException: failure);
            }

            if (request.ThumbnailBytes is { Length: > 0 })
            {
                await SetThumbnailAsync(
                    credentials,
                    uploadedVideo.Id,
                    request.ThumbnailBytes,
                    request.ThumbnailMimeType ?? "image/png",
                    cancellationToken).ConfigureAwait(false);
            }

            return MapVideo(uploadedVideo);
        }

        public async Task<VideoInfo> UpdateVideoAsync(SocialCredentials credentials, VideoUpdateRequest request, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var getRequest = service.Videos.List(VideoParts);
            getRequest.Id = request.VideoId;
            var existing = await ExecuteAsync(() => getRequest.ExecuteAsync(cancellationToken), nameof(UpdateVideoAsync)).ConfigureAwait(false);
            var video = existing.Items?.FirstOrDefault()
                ?? throw new NotFoundException($"Không tìm thấy video YouTube với Id '{request.VideoId}'.", "SOCIAL_YOUTUBE_VIDEO_NOT_FOUND");

            video.Snippet ??= new YTData.VideoSnippet();
            video.Status ??= new YTData.VideoStatus();

            if (request.Title is not null) video.Snippet.Title = request.Title;
            if (request.Description is not null) video.Snippet.Description = request.Description;
            if (request.Tags is not null) video.Snippet.Tags = request.Tags;
            if (request.CategoryId is not null) video.Snippet.CategoryId = request.CategoryId;
            if (request.PrivacyStatus is not null) video.Status.PrivacyStatus = MapPrivacyStatus(request.PrivacyStatus.Value);
            if (request.MadeForKids is not null) video.Status.SelfDeclaredMadeForKids = request.MadeForKids;
            if (request.PublishAt is not null) video.Status.PublishAtDateTimeOffset = request.PublishAt;

            var updateRequest = service.Videos.Update(video, "snippet,status");
            var updated = await ExecuteAsync(() => updateRequest.ExecuteAsync(cancellationToken), nameof(UpdateVideoAsync)).ConfigureAwait(false);
            return MapVideo(updated);
        }

        public async Task<bool> DeleteVideoAsync(SocialCredentials credentials, string videoId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Videos.Delete(videoId);
            await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(DeleteVideoAsync)).ConfigureAwait(false);
            return true;
        }

        public async Task<VideoInfo?> GetVideoByIdAsync(SocialCredentials credentials, string videoId, CancellationToken cancellationToken = default)
        {
            using var service = CreateService(credentials);
            var request = service.Videos.List(VideoParts);
            request.Id = videoId;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(GetVideoByIdAsync)).ConfigureAwait(false);
            return response.Items?.Select(MapVideo).FirstOrDefault();
        }

        public async Task<PagedResult<VideoInfo>> ListChannelVideosAsync(SocialCredentials credentials, VideoListRequest request, CancellationToken cancellationToken = default)
        {
            using var service = CreateService(credentials);

            // Cách hiệu quả nhất (ít tốn quota nhất) để liệt kê toàn bộ video của 1 channel là
            // đọc qua playlist "uploads" mặc định của channel đó, thay vì dùng Search.list
            // (Search tốn 100 quota units/request so với PlaylistItems.list chỉ tốn 1 unit).
            var channelRequest = service.Channels.List("contentDetails");
            channelRequest.Id = request.ChannelId;
            var channelResponse = await ExecuteAsync(() => channelRequest.ExecuteAsync(cancellationToken), nameof(ListChannelVideosAsync)).ConfigureAwait(false);

            var uploadsPlaylistId = channelResponse.Items?.FirstOrDefault()?.ContentDetails?.RelatedPlaylists?.Uploads;
            if (string.IsNullOrEmpty(uploadsPlaylistId))
            {
                return new PagedResult<VideoInfo> { Items = new List<VideoInfo>() };
            }

            var itemsRequest = service.PlaylistItems.List("contentDetails");
            itemsRequest.PlaylistId = uploadsPlaylistId;
            itemsRequest.MaxResults = request.MaxResults;
            itemsRequest.PageToken = request.PageToken;

            var itemsResponse = await ExecuteAsync(() => itemsRequest.ExecuteAsync(cancellationToken), nameof(ListChannelVideosAsync)).ConfigureAwait(false);
            var videoIds = itemsResponse.Items?.Select(i => i.ContentDetails.VideoId).Where(id => !string.IsNullOrEmpty(id)).ToList() ?? new List<string>();

            if (videoIds.Count == 0)
            {
                return new PagedResult<VideoInfo>
                {
                    Items = new List<VideoInfo>(),
                    NextPageToken = itemsResponse.NextPageToken,
                    PrevPageToken = itemsResponse.PrevPageToken,
                    TotalResults = itemsResponse.PageInfo?.TotalResults
                };
            }

            var videosRequest = service.Videos.List(VideoParts);
            videosRequest.Id = string.Join(",", videoIds);
            var videosResponse = await ExecuteAsync(() => videosRequest.ExecuteAsync(cancellationToken), nameof(ListChannelVideosAsync)).ConfigureAwait(false);

            return new PagedResult<VideoInfo>
            {
                Items = videosResponse.Items?.Select(MapVideo).ToList() ?? new List<VideoInfo>(),
                NextPageToken = itemsResponse.NextPageToken,
                PrevPageToken = itemsResponse.PrevPageToken,
                TotalResults = itemsResponse.PageInfo?.TotalResults
            };
        }

        public async Task<PagedResult<VideoInfo>> SearchVideosAsync(SocialCredentials credentials, VideoSearchRequest request, CancellationToken cancellationToken = default)
        {
            using var service = CreateService(credentials);
            var searchRequest = service.Search.List("snippet");
            searchRequest.Q = request.Query;
            searchRequest.ChannelId = request.ChannelId;
            searchRequest.Type = "video";
            searchRequest.MaxResults = request.MaxResults;
            searchRequest.PageToken = request.PageToken;
            searchRequest.Order = Enum.TryParse<SearchResource.ListRequest.OrderEnum>(request.Order, true, out var order)
                ? order
                : SearchResource.ListRequest.OrderEnum.Relevance;

            var searchResponse = await ExecuteAsync(() => searchRequest.ExecuteAsync(cancellationToken), nameof(SearchVideosAsync)).ConfigureAwait(false);
            var videoIds = searchResponse.Items?.Select(i => i.Id.VideoId).Where(id => !string.IsNullOrEmpty(id)).ToList() ?? new List<string>();

            if (videoIds.Count == 0)
            {
                return new PagedResult<VideoInfo>
                {
                    Items = new List<VideoInfo>(),
                    NextPageToken = searchResponse.NextPageToken,
                    PrevPageToken = searchResponse.PrevPageToken,
                    TotalResults = searchResponse.PageInfo?.TotalResults
                };
            }

            var videosRequest = service.Videos.List(VideoParts);
            videosRequest.Id = string.Join(",", videoIds);
            var videosResponse = await ExecuteAsync(() => videosRequest.ExecuteAsync(cancellationToken), nameof(SearchVideosAsync)).ConfigureAwait(false);

            return new PagedResult<VideoInfo>
            {
                Items = videosResponse.Items?.Select(MapVideo).ToList() ?? new List<VideoInfo>(),
                NextPageToken = searchResponse.NextPageToken,
                PrevPageToken = searchResponse.PrevPageToken,
                TotalResults = searchResponse.PageInfo?.TotalResults
            };
        }

        public async Task<string?> SetThumbnailAsync(SocialCredentials credentials, string videoId, byte[] imageBytes, string mimeType, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            using var stream = new MemoryStream(imageBytes);

            var setRequest = service.Thumbnails.Set(videoId, stream, mimeType);

            YTData.ThumbnailSetResponse? response = null;
            setRequest.ResponseReceived += r => response = r;

            var finalProgress = await ExecuteAsync(
                () => setRequest.UploadAsync(cancellationToken),
                nameof(SetThumbnailAsync)).ConfigureAwait(false);

            if (finalProgress.Status == UploadStatus.Failed)
            {
                throw new SocialProviderException(
                    CSocialMediaProviderType.Youtube,
                    StatusCodes.Status502BadGateway,
                    finalProgress.Exception?.Message ?? "Đặt thumbnail cho video YouTube thất bại.",
                    innerException: finalProgress.Exception);
            }

            var thumbnail = response?.Items?.FirstOrDefault()?.Default__;
            return thumbnail?.Url;
        }

        public async Task RateVideoAsync(SocialCredentials credentials, string videoId, VideoRatingType rating, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Videos.Rate(videoId, MapRating(rating));
            await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(RateVideoAsync)).ConfigureAwait(false);
        }

        public async Task<VideoRatingType> GetMyRatingAsync(SocialCredentials credentials, string videoId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Videos.GetRating(videoId);
            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(GetMyRatingAsync)).ConfigureAwait(false);

            var ratingText = response.Items?.FirstOrDefault()?.Rating;
            return ratingText switch
            {
                "like" => VideoRatingType.Like,
                "dislike" => VideoRatingType.Dislike,
                _ => VideoRatingType.None
            };
        }

        // ============================================================
        // IPlaylistManagementCapableEngine
        // ============================================================

        public async Task<PlaylistInfo> CreatePlaylistAsync(SocialCredentials credentials, PlaylistCreateRequest request, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var playlist = new YTData.Playlist
            {
                Snippet = new YTData.PlaylistSnippet
                {
                    Title = request.Title,
                    Description = request.Description,
                    Tags = request.Tags
                },
                Status = new YTData.PlaylistStatus
                {
                    PrivacyStatus = MapPrivacyStatus(request.PrivacyStatus)
                }
            };

            var insertRequest = service.Playlists.Insert(playlist, PlaylistParts);
            var created = await ExecuteAsync(() => insertRequest.ExecuteAsync(cancellationToken), nameof(CreatePlaylistAsync)).ConfigureAwait(false);
            return MapPlaylist(created);
        }

        public async Task<PlaylistInfo> UpdatePlaylistAsync(SocialCredentials credentials, PlaylistUpdateRequest request, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var getRequest = service.Playlists.List(PlaylistParts);
            getRequest.Id = request.PlaylistId;
            var existing = await ExecuteAsync(() => getRequest.ExecuteAsync(cancellationToken), nameof(UpdatePlaylistAsync)).ConfigureAwait(false);
            var playlist = existing.Items?.FirstOrDefault()
                ?? throw new NotFoundException($"Không tìm thấy playlist YouTube với Id '{request.PlaylistId}'.", "SOCIAL_YOUTUBE_PLAYLIST_NOT_FOUND");

            playlist.Snippet ??= new YTData.PlaylistSnippet();
            playlist.Status ??= new YTData.PlaylistStatus();

            if (request.Title is not null) playlist.Snippet.Title = request.Title;
            if (request.Description is not null) playlist.Snippet.Description = request.Description;
            if (request.PrivacyStatus is not null) playlist.Status.PrivacyStatus = MapPrivacyStatus(request.PrivacyStatus.Value);

            var updateRequest = service.Playlists.Update(playlist, PlaylistParts);
            var updated = await ExecuteAsync(() => updateRequest.ExecuteAsync(cancellationToken), nameof(UpdatePlaylistAsync)).ConfigureAwait(false);
            return MapPlaylist(updated);
        }

        public async Task<bool> DeletePlaylistAsync(SocialCredentials credentials, string playlistId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Playlists.Delete(playlistId);
            await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(DeletePlaylistAsync)).ConfigureAwait(false);
            return true;
        }

        public async Task<PagedResult<PlaylistInfo>> ListMyPlaylistsAsync(SocialCredentials credentials, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Playlists.List(PlaylistParts);
            request.Mine = true;
            request.MaxResults = maxResults;
            request.PageToken = pageToken;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(ListMyPlaylistsAsync)).ConfigureAwait(false);

            return new PagedResult<PlaylistInfo>
            {
                Items = response.Items?.Select(MapPlaylist).ToList() ?? new List<PlaylistInfo>(),
                NextPageToken = response.NextPageToken,
                PrevPageToken = response.PrevPageToken,
                TotalResults = response.PageInfo?.TotalResults
            };
        }

        public async Task<PlaylistItemInfo> AddVideoToPlaylistAsync(SocialCredentials credentials, string playlistId, string videoId, int? position = null, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var item = new YTData.PlaylistItem
            {
                Snippet = new YTData.PlaylistItemSnippet
                {
                    PlaylistId = playlistId,
                    Position = position,
                    ResourceId = new YTData.ResourceId
                    {
                        Kind = "youtube#video",
                        VideoId = videoId
                    }
                }
            };

            var insertRequest = service.PlaylistItems.Insert(item, PlaylistItemParts);
            var created = await ExecuteAsync(() => insertRequest.ExecuteAsync(cancellationToken), nameof(AddVideoToPlaylistAsync)).ConfigureAwait(false);
            return MapPlaylistItem(created);
        }

        public async Task<bool> RemovePlaylistItemAsync(SocialCredentials credentials, string playlistItemId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.PlaylistItems.Delete(playlistItemId);
            await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(RemovePlaylistItemAsync)).ConfigureAwait(false);
            return true;
        }

        public async Task<PagedResult<PlaylistItemInfo>> ListPlaylistItemsAsync(SocialCredentials credentials, string playlistId, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default)
        {
            using var service = CreateService(credentials);
            var request = service.PlaylistItems.List(PlaylistItemParts + ",snippet");
            request.PlaylistId = playlistId;
            request.MaxResults = maxResults;
            request.PageToken = pageToken;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(ListPlaylistItemsAsync)).ConfigureAwait(false);

            return new PagedResult<PlaylistItemInfo>
            {
                Items = response.Items?.Select(MapPlaylistItem).ToList() ?? new List<PlaylistItemInfo>(),
                NextPageToken = response.NextPageToken,
                PrevPageToken = response.PrevPageToken,
                TotalResults = response.PageInfo?.TotalResults
            };
        }

        // ============================================================
        // ICommentManagementCapableEngine
        // ============================================================

        public async Task<PagedResult<CommentThreadInfo>> ListCommentThreadsAsync(SocialCredentials credentials, string videoId, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default)
        {
            using var service = CreateService(credentials);
            var request = service.CommentThreads.List(CommentThreadParts);
            request.VideoId = videoId;
            request.MaxResults = maxResults;
            request.PageToken = pageToken;
            request.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(ListCommentThreadsAsync)).ConfigureAwait(false);

            return new PagedResult<CommentThreadInfo>
            {
                Items = response.Items?.Select(MapCommentThread).ToList() ?? new List<CommentThreadInfo>(),
                // CommentThreadListResponse chỉ có nextPageToken, KHÔNG có prevPageToken
                // (khác với Channels/Playlists/Videos/Search/Subscriptions/LiveBroadcasts).
                NextPageToken = response.NextPageToken,
                TotalResults = response.PageInfo?.TotalResults
            };
        }

        public async Task<CommentThreadInfo> InsertTopLevelCommentAsync(SocialCredentials credentials, string videoId, string text, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var commentThread = new YTData.CommentThread
            {
                Snippet = new YTData.CommentThreadSnippet
                {
                    VideoId = videoId,
                    TopLevelComment = new YTData.Comment
                    {
                        Snippet = new YTData.CommentSnippet { TextOriginal = text }
                    }
                }
            };

            var insertRequest = service.CommentThreads.Insert(commentThread, CommentThreadParts);
            var created = await ExecuteAsync(() => insertRequest.ExecuteAsync(cancellationToken), nameof(InsertTopLevelCommentAsync)).ConfigureAwait(false);
            return MapCommentThread(created);
        }

        public async Task<CommentInfo> ReplyToCommentAsync(SocialCredentials credentials, string parentCommentId, string text, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var comment = new YTData.Comment
            {
                Snippet = new YTData.CommentSnippet
                {
                    ParentId = parentCommentId,
                    TextOriginal = text
                }
            };

            var insertRequest = service.Comments.Insert(comment, CommentParts);
            var created = await ExecuteAsync(() => insertRequest.ExecuteAsync(cancellationToken), nameof(ReplyToCommentAsync)).ConfigureAwait(false);
            return MapComment(created);
        }

        public async Task SetCommentModerationStatusAsync(SocialCredentials credentials, string commentId, CommentModerationStatus status, bool banAuthor = false, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Comments.SetModerationStatus(commentId, MapModerationStatus(status));
            request.BanAuthor = banAuthor;
            await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(SetCommentModerationStatusAsync)).ConfigureAwait(false);
        }

        public async Task<bool> DeleteCommentAsync(SocialCredentials credentials, string commentId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Comments.Delete(commentId);
            await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(DeleteCommentAsync)).ConfigureAwait(false);
            return true;
        }

        // ============================================================
        // ILiveStreamingCapableEngine
        // ============================================================

        public async Task<LiveBroadcastInfo> CreateLiveBroadcastAsync(SocialCredentials credentials, LiveBroadcastCreateRequest request, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var broadcast = new YTData.LiveBroadcast
            {
                Snippet = new YTData.LiveBroadcastSnippet
                {
                    Title = request.Title,
                    Description = request.Description,
                    ScheduledStartTimeDateTimeOffset = request.ScheduledStartTime,
                    ScheduledEndTimeDateTimeOffset = request.ScheduledEndTime
                },
                Status = new YTData.LiveBroadcastStatus
                {
                    PrivacyStatus = MapPrivacyStatus(request.PrivacyStatus),
                    SelfDeclaredMadeForKids = request.MadeForKids
                },
                ContentDetails = new YTData.LiveBroadcastContentDetails
                {
                    EnableAutoStart = request.EnableAutoStart,
                    EnableAutoStop = request.EnableAutoStop,
                    EnableDvr = request.EnableDvr
                }
            };

            var insertRequest = service.LiveBroadcasts.Insert(broadcast, LiveBroadcastParts);
            var created = await ExecuteAsync(() => insertRequest.ExecuteAsync(cancellationToken), nameof(CreateLiveBroadcastAsync)).ConfigureAwait(false);
            return MapBroadcast(created);
        }

        public async Task<LiveStreamInfo> CreateLiveStreamAsync(SocialCredentials credentials, LiveStreamCreateRequest request, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var stream = new YTData.LiveStream
            {
                Snippet = new YTData.LiveStreamSnippet { Title = request.Title },
                Cdn = new YTData.CdnSettings
                {
                    Resolution = request.Resolution,
                    FrameRate = request.FrameRate,
                    IngestionType = request.IngestionType
                }
            };

            var insertRequest = service.LiveStreams.Insert(stream, LiveStreamParts);
            var created = await ExecuteAsync(() => insertRequest.ExecuteAsync(cancellationToken), nameof(CreateLiveStreamAsync)).ConfigureAwait(false);
            return MapStream(created);
        }

        public async Task<LiveBroadcastInfo> BindBroadcastToStreamAsync(SocialCredentials credentials, string broadcastId, string streamId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.LiveBroadcasts.Bind(broadcastId, LiveBroadcastParts);
            request.StreamId = streamId;

            var result = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(BindBroadcastToStreamAsync)).ConfigureAwait(false);
            return MapBroadcast(result);
        }

        public async Task<LiveBroadcastInfo> TransitionBroadcastAsync(SocialCredentials credentials, string broadcastId, LiveBroadcastStatus targetStatus, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.LiveBroadcasts.Transition(MapTransitionStatus(targetStatus), broadcastId, LiveBroadcastParts);

            var result = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(TransitionBroadcastAsync)).ConfigureAwait(false);
            return MapBroadcast(result);
        }

        public async Task<PagedResult<LiveBroadcastInfo>> ListMyBroadcastsAsync(SocialCredentials credentials, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.LiveBroadcasts.List(LiveBroadcastParts);
            request.Mine = true;
            request.MaxResults = maxResults;
            request.PageToken = pageToken;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(ListMyBroadcastsAsync)).ConfigureAwait(false);

            return new PagedResult<LiveBroadcastInfo>
            {
                Items = response.Items?.Select(MapBroadcast).ToList() ?? new List<LiveBroadcastInfo>(),
                NextPageToken = response.NextPageToken,
                PrevPageToken = response.PrevPageToken,
                TotalResults = response.PageInfo?.TotalResults
            };
        }

        public async Task<bool> DeleteBroadcastAsync(SocialCredentials credentials, string broadcastId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.LiveBroadcasts.Delete(broadcastId);
            await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(DeleteBroadcastAsync)).ConfigureAwait(false);
            return true;
        }

        // ============================================================
        // ISubscriptionManagementCapableEngine
        // ============================================================

        public async Task<SubscriptionInfo> SubscribeAsync(SocialCredentials credentials, string channelId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);

            var subscription = new YTData.Subscription
            {
                Snippet = new YTData.SubscriptionSnippet
                {
                    ResourceId = new YTData.ResourceId
                    {
                        Kind = "youtube#channel",
                        ChannelId = channelId
                    }
                }
            };

            var insertRequest = service.Subscriptions.Insert(subscription, SubscriptionParts);
            var created = await ExecuteAsync(() => insertRequest.ExecuteAsync(cancellationToken), nameof(SubscribeAsync)).ConfigureAwait(false);
            return MapSubscription(created);
        }

        public async Task<bool> UnsubscribeAsync(SocialCredentials credentials, string subscriptionId, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Subscriptions.Delete(subscriptionId);
            await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(UnsubscribeAsync)).ConfigureAwait(false);
            return true;
        }

        public async Task<PagedResult<SubscriptionInfo>> ListMySubscriptionsAsync(SocialCredentials credentials, int maxResults = 25, string? pageToken = null, CancellationToken cancellationToken = default)
        {
            using var service = YoutubeCredentialFactory.CreateOAuthService(credentials);
            var request = service.Subscriptions.List(SubscriptionParts);
            request.Mine = true;
            request.MaxResults = maxResults;
            request.PageToken = pageToken;

            var response = await ExecuteAsync(() => request.ExecuteAsync(cancellationToken), nameof(ListMySubscriptionsAsync)).ConfigureAwait(false);

            return new PagedResult<SubscriptionInfo>
            {
                Items = response.Items?.Select(MapSubscription).ToList() ?? new List<SubscriptionInfo>(),
                NextPageToken = response.NextPageToken,
                PrevPageToken = response.PrevPageToken,
                TotalResults = response.PageInfo?.TotalResults
            };
        }

        // ============================================================
        // Helpers - mapping Data model -> DTO nội bộ
        // ============================================================

        private static ChannelInfo MapChannel(YTData.Channel c) => new()
        {
            ChannelId = c.Id,
            Title = c.Snippet?.Title ?? string.Empty,
            Description = c.Snippet?.Description,
            CustomUrl = c.Snippet?.CustomUrl,
            ThumbnailUrl = c.Snippet?.Thumbnails?.High?.Url ?? c.Snippet?.Thumbnails?.Default__?.Url,
            SubscriberCount = (long?)c.Statistics?.SubscriberCount,
            VideoCount = (long?)c.Statistics?.VideoCount,
            ViewCount = (long?)c.Statistics?.ViewCount,
            HiddenSubscriberCount = c.Statistics?.HiddenSubscriberCount ?? false,
            UploadsPlaylistId = c.ContentDetails?.RelatedPlaylists?.Uploads,
            PublishedAt = c.Snippet?.PublishedAtDateTimeOffset,
            Country = c.Snippet?.Country
        };

        private static VideoInfo MapVideo(YTData.Video v) => new()
        {
            VideoId = v.Id,
            ChannelId = v.Snippet?.ChannelId ?? string.Empty,
            Title = v.Snippet?.Title ?? string.Empty,
            Description = v.Snippet?.Description,
            Tags = v.Snippet?.Tags?.ToList() ?? new List<string>(),
            CategoryId = v.Snippet?.CategoryId,
            PrivacyStatus = MapPrivacyStatus(v.Status?.PrivacyStatus),
            ThumbnailUrl = v.Snippet?.Thumbnails?.High?.Url ?? v.Snippet?.Thumbnails?.Default__?.Url,
            UploadStatus = v.Status?.UploadStatus,
            ProcessingStatus = v.ProcessingDetails?.ProcessingStatus,
            Duration = ParseIso8601Duration(v.ContentDetails?.Duration),
            ViewCount = (long?)v.Statistics?.ViewCount,
            LikeCount = (long?)v.Statistics?.LikeCount,
            CommentCount = (long?)v.Statistics?.CommentCount,
            PublishedAt = v.Snippet?.PublishedAtDateTimeOffset
        };

        private static PlaylistInfo MapPlaylist(YTData.Playlist p) => new()
        {
            PlaylistId = p.Id,
            ChannelId = p.Snippet?.ChannelId ?? string.Empty,
            Title = p.Snippet?.Title ?? string.Empty,
            Description = p.Snippet?.Description,
            PrivacyStatus = MapPrivacyStatus(p.Status?.PrivacyStatus),
            ItemCount = (long?)p.ContentDetails?.ItemCount,
            ThumbnailUrl = p.Snippet?.Thumbnails?.High?.Url ?? p.Snippet?.Thumbnails?.Default__?.Url
        };

        private static PlaylistItemInfo MapPlaylistItem(YTData.PlaylistItem i) => new()
        {
            PlaylistItemId = i.Id,
            PlaylistId = i.Snippet?.PlaylistId ?? string.Empty,
            VideoId = i.ContentDetails?.VideoId ?? i.Snippet?.ResourceId?.VideoId ?? string.Empty,
            Title = i.Snippet?.Title ?? string.Empty,
            Position = (int)(i.Snippet?.Position ?? 0),
            AddedAt = i.Snippet?.PublishedAtDateTimeOffset
        };

        private static CommentInfo MapComment(YTData.Comment c) => new()
        {
            CommentId = c.Id,
            AuthorDisplayName = c.Snippet?.AuthorDisplayName ?? string.Empty,
            AuthorChannelId = c.Snippet?.AuthorChannelId?.Value,
            TextDisplay = c.Snippet?.TextDisplay ?? c.Snippet?.TextOriginal ?? string.Empty,
            LikeCount = c.Snippet?.LikeCount ?? 0,
            CanRate = c.Snippet?.CanRate ?? false,
            PublishedAt = c.Snippet?.PublishedAtDateTimeOffset,
            UpdatedAt = c.Snippet?.UpdatedAtDateTimeOffset
        };

        private static CommentThreadInfo MapCommentThread(YTData.CommentThread t) => new()
        {
            CommentThreadId = t.Id,
            VideoId = t.Snippet?.VideoId ?? string.Empty,
            TopLevelComment = MapComment(t.Snippet!.TopLevelComment),
            ReplyCount = t.Snippet?.TotalReplyCount ?? 0,
            IsPublic = t.Snippet?.IsPublic ?? true,
            Replies = t.Replies?.Comments?.Select(MapComment).ToList() ?? new List<CommentInfo>()
        };

        private static LiveBroadcastInfo MapBroadcast(YTData.LiveBroadcast b) => new()
        {
            BroadcastId = b.Id,
            Title = b.Snippet?.Title ?? string.Empty,
            Description = b.Snippet?.Description,
            Status = MapBroadcastLifeCycle(b.Status?.LifeCycleStatus),
            ScheduledStartTime = b.Snippet?.ScheduledStartTimeDateTimeOffset,
            ScheduledEndTime = b.Snippet?.ScheduledEndTimeDateTimeOffset,
            ActualStartTime = b.Snippet?.ActualStartTimeDateTimeOffset,
            ActualEndTime = b.Snippet?.ActualEndTimeDateTimeOffset,
            BoundStreamId = b.ContentDetails?.BoundStreamId
        };

        private static LiveStreamInfo MapStream(YTData.LiveStream s) => new()
        {
            StreamId = s.Id,
            Title = s.Snippet?.Title ?? string.Empty,
            IngestionAddress = s.Cdn?.IngestionInfo?.IngestionAddress,
            StreamKey = s.Cdn?.IngestionInfo?.StreamName,
            Status = s.Status?.StreamStatus
        };

        private static SubscriptionInfo MapSubscription(YTData.Subscription s) => new()
        {
            SubscriptionId = s.Id,
            ChannelId = s.Snippet?.ResourceId?.ChannelId ?? string.Empty,
            ChannelTitle = s.Snippet?.Title ?? string.Empty,
            SubscribedAt = s.Snippet?.PublishedAtDateTimeOffset
        };

        // ============================================================
        // Helpers - mapping enum <-> string theo chuẩn YouTube API
        // ============================================================

        private static string MapPrivacyStatus(VideoPrivacyStatus status) => status switch
        {
            VideoPrivacyStatus.Public => "public",
            VideoPrivacyStatus.Unlisted => "unlisted",
            _ => "private"
        };

        private static VideoPrivacyStatus MapPrivacyStatus(string? status) => status switch
        {
            "public" => VideoPrivacyStatus.Public,
            "unlisted" => VideoPrivacyStatus.Unlisted,
            _ => VideoPrivacyStatus.Private
        };

        private static VideosResource.RateRequest.RatingEnum MapRating(VideoRatingType rating) => rating switch
        {
            VideoRatingType.Like => VideosResource.RateRequest.RatingEnum.Like,
            VideoRatingType.Dislike => VideosResource.RateRequest.RatingEnum.Dislike,
            _ => VideosResource.RateRequest.RatingEnum.None
        };

        private static CommentsResource.SetModerationStatusRequest.ModerationStatusEnum MapModerationStatus(CommentModerationStatus status) => status switch
        {
            CommentModerationStatus.Published => CommentsResource.SetModerationStatusRequest.ModerationStatusEnum.Published,
            CommentModerationStatus.HeldForReview => CommentsResource.SetModerationStatusRequest.ModerationStatusEnum.HeldForReview,
            CommentModerationStatus.Rejected => CommentsResource.SetModerationStatusRequest.ModerationStatusEnum.Rejected,
            // YouTube API không có trạng thái "spam" riêng cho setModerationStatus (chỉ hỗ trợ
            // heldForReview/published/rejected) - coi Spam như 1 dạng Rejected.
            CommentModerationStatus.Spam => CommentsResource.SetModerationStatusRequest.ModerationStatusEnum.Rejected,
            _ => CommentsResource.SetModerationStatusRequest.ModerationStatusEnum.Published
        };

        /// <summary>
        /// Map sang enum dùng riêng cho LiveBroadcasts.Transition - API này chỉ chấp nhận
        /// đúng 3 giá trị đích: testing, live, complete (không có ready/created/revoked).
        /// </summary>
        private static LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum MapTransitionStatus(LiveBroadcastStatus status) => status switch
        {
            LiveBroadcastStatus.Testing => LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Testing,
            LiveBroadcastStatus.Live => LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Live,
            LiveBroadcastStatus.Complete => LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Complete,
            _ => throw new ValidationException(
                $"Trạng thái đích '{status}' không hợp lệ cho việc transition livestream. Chỉ hỗ trợ: Testing, Live, Complete.",
                "SOCIAL_YOUTUBE_INVALID_TRANSITION_STATUS")
        };

        private static LiveBroadcastStatus MapBroadcastLifeCycle(string? status) => status switch
        {
            "ready" => LiveBroadcastStatus.Ready,
            "testing" or "testStarting" => LiveBroadcastStatus.Testing,
            "live" or "liveStarting" => LiveBroadcastStatus.Live,
            "complete" => LiveBroadcastStatus.Complete,
            "revoked" => LiveBroadcastStatus.Revoked,
            _ => LiveBroadcastStatus.Created
        };

        private static TimeSpan? ParseIso8601Duration(string? iso8601Duration)
        {
            if (string.IsNullOrEmpty(iso8601Duration))
                return null;

            try
            {
                return System.Xml.XmlConvert.ToTimeSpan(iso8601Duration);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        // ============================================================
        // Helpers - lựa chọn service & xử lý lỗi
        // ============================================================

        private static YouTubeService CreateService(SocialCredentials credentials) =>
            YoutubeCredentialFactory.CreateService(credentials);

        /// <summary>
        /// Thực thi 1 lệnh gọi YouTube API, bắt <see cref="GoogleApiException"/> và chuẩn hóa
        /// thành <see cref="SocialProviderException"/> để tầng trên xử lý đồng nhất với các
        /// social provider khác (Facebook, Tiktok, X...).
        /// </summary>
        private async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, string actionName)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (GoogleApiException ex)
            {
                var firstError = ex.Error?.Errors?.FirstOrDefault();
                var statusCode = (int)(ex.HttpStatusCode == default ? System.Net.HttpStatusCode.BadGateway : ex.HttpStatusCode);

                var exception = new SocialProviderException(
                    CSocialMediaProviderType.Youtube,
                    statusCode,
                    ex.Error?.Message ?? ex.Message,
                    providerErrorReason: firstError?.Reason,
                    providerErrorDomain: firstError?.Domain,
                    innerException: ex);

                _logger.LogError(exception,
                    "YouTube API call failed. Action: {ActionName}, HTTP Status: {HttpStatusCode}, Reason: {Reason}, Message: {ErrorMessage}",
                    actionName, exception.ProviderStatusCode, exception.ProviderErrorReason ?? "N/A", exception.ProviderMessage);

                throw exception;
            }
        }
    }
}