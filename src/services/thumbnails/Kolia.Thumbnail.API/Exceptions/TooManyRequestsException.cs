namespace Kolia.Thumbnail.API.Exceptions
{
    /// <summary>
    /// Đại diện cho ngoại lệ khi vượt quá giới hạn số lượng yêu cầu.
    /// </summary>
    public sealed class TooManyRequestsException : AppException
    {
        public TooManyRequestsException(
            string message = "Too many requests.",
            string code = "TOO_MANY_REQUESTS")
            : base(message, code, StatusCodes.Status429TooManyRequests)
        {
        }

        public TooManyRequestsException(
            string message,
            Exception innerException,
            string code = "TOO_MANY_REQUESTS")
            : base(message, innerException, code, StatusCodes.Status429TooManyRequests)
        {
        }
    }
}