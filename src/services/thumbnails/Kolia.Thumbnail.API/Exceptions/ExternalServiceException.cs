namespace Kolia.Thumbnail.API.Exceptions
{
    /// <summary>
    /// Đại diện cho ngoại lệ khi dịch vụ bên ngoài không khả dụng hoặc trả về lỗi.
    /// </summary>
    public sealed class ExternalServiceException : AppException
    {
        public ExternalServiceException(
            string message = "External service is unavailable.",
            string code = "EXTERNAL_SERVICE_ERROR")
            : base(message, code, StatusCodes.Status502BadGateway)
        {
        }

        public ExternalServiceException(
            string message,
            Exception innerException,
            string code = "EXTERNAL_SERVICE_ERROR")
            : base(message, innerException, code, StatusCodes.Status502BadGateway)
        {
        }
    }
}