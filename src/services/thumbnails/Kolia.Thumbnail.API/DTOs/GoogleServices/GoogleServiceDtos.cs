namespace Kolia.Thumbnail.API.DTOs.GoogleServices
{
    /// <summary>
    /// Request tạo mới Google Service Account từ file JSON credential.
    /// </summary>
    public record CreateGoogleServiceAccountRequest(
        string Name,
        string? Description,
        string CredentialJson,
        string? Scopes);

    /// <summary>
    /// Request cập nhật Google Service Account.
    /// </summary>
    public record UpdateGoogleServiceAccountRequest(
        string Name,
        string? Description,
        string? CredentialJson,
        string? Scopes,
        bool IsEnabled);

    /// <summary>
    /// DTO chi tiết Google Service Account (trả về cho client).
    /// Các trường nhạy cảm như PrivateKey và RawCredentialJson KHÔNG được trả về.
    /// </summary>
    public record GoogleServiceAccountDto(
        Guid Id,
        string Name,
        string? Description,
        string ClientEmail,
        string? ClientId,
        string? ProjectId,
        string? TokenUri,
        string? Scopes,
        bool IsEnabled,
        DateTimeOffset CreationTime,
        DateTimeOffset? LastModificationTime);

    /// <summary>
    /// DTO summary cho danh sách.
    /// </summary>
    public record GoogleServiceAccountSummaryDto(
        Guid Id,
        string Name,
        string ClientEmail,
        string? ProjectId,
        bool IsEnabled,
        int TotalJobs,
        DateTimeOffset CreationTime);
}
